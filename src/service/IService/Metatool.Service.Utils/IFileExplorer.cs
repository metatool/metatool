﻿using System;
using System.Threading.Tasks;

namespace Metatool.Service
{
    public interface IFileExplorer
    {
        Task<string[]> GetSelectedPaths(IntPtr hWnd);
        Task<string> Path(IntPtr hWnd);
        void Select(IntPtr hWnd, string[] fileNames);
        string Open(string path);
    }
}