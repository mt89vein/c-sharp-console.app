using System;
using System.Collections.Generic;
using System.IO;
using Autofac.Core;

namespace dirhash
{
    /// <summary>
    /// класс для обхода дерева каталогов 
    /// запрашивает от юзера какую директорию нужно обойти, потом обходит и создает очередь из файлов
    /// </summary>
    public class DirectoryWalker
    {
        public string RootPath { get; set; }
        public bool IsExistPath { get; set; }

        public string SelectPath()
        {
            do
            {
                Console.Write("Введите путь: ");

                RootPath = Console.ReadLine();
                IsExistPath = Directory.Exists(RootPath);

                Console.WriteLine(!IsExistPath ? "Путь не найден." : $"Выбрана директория: {RootPath}");

            } while (!IsExistPath);


            return RootPath;
        }

        public void Walk(ref Queue<FileInfo> filesQueue, ref int count)
        {
            Program.Added += Program.StatusVerboser;
            Walk(RootPath, ref filesQueue, ref count);
        }

        private void Walk(string path, ref Queue<FileInfo> filesQueue, ref int count)
        {
            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = null;

            try
            {
                files = root.GetFiles("*.*");
            }
            catch (Exception e)
            {
                throw e;
            }


            foreach (FileInfo file in files)
            {
                lock (filesQueue)
                {
                    filesQueue.Enqueue(file);
                    count++;
                }
            }


            var subDirs = root.GetDirectories();

            foreach (DirectoryInfo dirInfo in subDirs)
            {
                Walk(dirInfo.FullName, ref filesQueue, ref count);
            }

        }


    }
}
