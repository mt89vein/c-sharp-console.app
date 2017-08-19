using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using dirhash.Services;
using Autofac;
using dirhash.Models;

namespace dirhash
{
    class Program
    {
        #region [properties]
        #region [События]
        /// <summary>
        /// Делегат для вывода текущего статуса выполнения
        /// </summary>
        /// <param name="current"></param>
        /// <param name="hashed"></param>
        /// <param name="all"></param>
        private delegate void FilesStateHandler(int current, int hashed, int all);
        /// <summary>
        /// Событие возникает при добавлении в очереди или запись в бд
        /// </summary>
        private static event FilesStateHandler Added;

        #endregion [События]

        #region [Очереди]

        private static readonly Queue<KeyValuePair<string, string>> FilesHashQueue = new Queue<KeyValuePair<string, string>>();
        private static readonly Queue<FileInfo> FilesInfoQueue = new Queue<FileInfo>();

        #endregion [Очереди]

        #region [Счётчики]

        public static int SavedCount { get; set; }
        public static int HashedCount { get; set; }
        public static int AllFilesCount { get; set; }

        #endregion [Счётчики]

        #region [DirectoryWalkerProperties]

        private static string RootPath { get; set; }
        private static bool IsExistPath { get; set; }

        #endregion [DirectoryWalkerProperties]

        #region [DiContainer]

        private static ILogService _logService;
        private static Autofac.IContainer _container;
        private static IFileService _fileService;

        private static ILogService LogService
        {
            get
            {
                if (_logService == null)
                    return _logService = Container.Resolve<ILogService>();
                return _logService;
            }
        }

        private static IFileService FileService
        {
            get
            {
                if (_fileService == null)
                    return _fileService = Container.Resolve<IFileService>();
                return _fileService;
            }
        }

        private static Autofac.IContainer Container
        {
            get
            {
                if (_container == null)
                    return _container = new IoCConfig().Build();
                return _container;
            }
        }
        
        #endregion [DiContainer]

        #endregion [properties]

        #region [Workers]

        /// <summary>
        /// Смотрит появились ли записи в хэш очереди и записывает их в бд пачками до 30 шт
        /// </summary>
        private static void DbWorker()
        {
            while (true)
            {
                if (FilesHashQueue.Count == 0)
                    Thread.Sleep(1);
                else
                    try
                    {
                        List<FileEntity> toInsertEntities = new List<FileEntity>();
                        lock (FilesHashQueue)
                        {
                            //вставляем пачками по 30 шт или сколько там набралось, бережём бд :)
                            for (int i = 0; i <= Math.Min(29, FilesHashQueue.Count); i++)
                            {
                                KeyValuePair<string, string> file = FilesHashQueue.Dequeue();

                                if (file.Value != string.Empty)
                                {
                                    toInsertEntities.Add(new FileEntity()
                                    {
                                        CreatedAt = DateTime.Now,
                                        Filename = file.Key,
                                        Hash = file.Value
                                    });
                                }
                            }
                        }
                        if (FileService.Insert(toInsertEntities))
                        {
                            SavedCount += toInsertEntities.Count;
                            Added?.Invoke(SavedCount, 0, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.Add(e.Message);
                    }
            }
        }

        /// <summary>
        /// Смотрит появились ли записи в очереди файлов на вычисление хэш суммы, вычисляет и ставит в очередь на запись в бд
        /// </summary>
        private static void HashWorker()
        {
            while (true)
            {
                if (FilesInfoQueue.Count == 0)
                    Thread.Sleep(1);
                else
                    try
                    {
                        FileInfo file;
                        lock (FilesInfoQueue)
                        {
                            file = FilesInfoQueue.Dequeue();
                        }

                        string hash = GetFileHash(file.FullName);
                        if (hash != string.Empty)
                        {
                            lock (FilesHashQueue)
                            {
                                FilesHashQueue.Enqueue(new KeyValuePair<string, string>(file.Name, hash));
                            }
                            Added?.Invoke(0, ++HashedCount, 0);
                        }
                    }
                    catch (Exception e)
                    {
                        LogService.Add(e.Message);
                    }
            }

        }

        #region [DirectoryWalker]

        private static void SelectPath()
        {
            do
            {
                Console.Write("Введите путь: ");

                RootPath = Console.ReadLine();
                IsExistPath = Directory.Exists(RootPath);

                Console.WriteLine(!IsExistPath ? "Путь не найден." : $"Выбрана директория: {RootPath}");

            } while (!IsExistPath);
        }

        static void Walker()
        {
            Walk(RootPath);
        }

        static void Walk(string path)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = null;

            try
            {
                files = root.GetFiles("*.*");
            }
            catch (Exception e)
            {
                LogService.Add(e.Message);
            }


            if (files != null)

                foreach (FileInfo file in files)
                {
                    lock (FilesInfoQueue)
                    {
                        FilesInfoQueue.Enqueue(file);
                    }
                    Added?.Invoke(0, 0, ++AllFilesCount);

                }

            var subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                Walk(dirInfo.FullName);
            }
        }

        #endregion
        #endregion [Workers]

        #region [Heplers]

        private static string GetFileHash(string filename)
        {
            string result = "";
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    using (FileStream stream = File.OpenRead(filename))
                    {
                        result = Convert.ToBase64String(md5.ComputeHash(inputStream: stream));
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Add(e.Message);
            }
            return result;
        }

        private static void StatusVerboser(int current = 0, int hashed = 0, int all = 0)
        {

            if (current == 0)
                current = SavedCount;

            if (hashed == 0)
                hashed = HashedCount;

            if (all == 0)
                all = AllFilesCount;
            Console.WriteLine($"left: {hashed - current,3} |  | hashed: {hashed,3} | all: {all,3}| done: {current,3}");
        }

        #endregion [Heplers]

        public static void Main(string[] args)
        {
            SelectPath();
            //при любых действиях выводим сообщение о статус
            Added += StatusVerboser;
            Thread walker = new Thread(Walker);
            walker.Start();

            Thread hasher = new Thread(HashWorker);
            hasher.Start();

            Thread dbWriter = new Thread(DbWorker);
            dbWriter.Start();
            
            Console.ReadKey();
        }
    }

}