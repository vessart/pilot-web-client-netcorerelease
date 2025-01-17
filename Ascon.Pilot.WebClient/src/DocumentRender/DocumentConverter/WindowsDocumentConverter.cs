﻿using System;
using System.Collections.Generic;
using System.IO;

namespace DocumentRender.DocumentConverter
{
    internal class WindowsDocumentConverter : IDocumentConverter
    {
        public byte[] ConvertPage(string fileName, int page)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return RenderFirstPageInBytes(stream, page);
            }
        }

        public byte[] ConvertPage(byte[] content, int page)
        {
            using (var stream = new MemoryStream(content))
            {
                return RenderFirstPageInBytes(stream, page);
            }
        }

        public IEnumerable<byte[]> ConvertFile(byte[] content)
        {
            using (var stream = new MemoryStream(content))
            {
                return LoadPages(stream);
            }
        }

        private byte[] RenderFirstPageInBytes(Stream xpsStream, int page)
        {
            using (var tileManager = new TilesManager(xpsStream))
            {
                return LoadPage(tileManager, page);
            }
        }

        private static byte[] LoadPage(TilesManager tileManager, int page)
        {
            var internalPage = page;
            if (page > 0)
                internalPage = page - 1;

            int pageWidth = 0, pageHeight = 0;
            tileManager.LoadPage(internalPage, ref pageWidth, ref pageHeight, false);
            var scale = 0.5;
            var tile = new Tile
            {
                PageNum = internalPage,
                Height = (int) (pageHeight * scale),
                Width = (int) (pageWidth * scale),
                Scale = scale
            };
            var result = tileManager.GetPageInBytes(tile, false);
            return result;
        }

        private IEnumerable<byte[]> LoadPages(Stream xpsStream)
        {
            var result = new List<byte[]>();
            using (var tileManager = new TilesManager(xpsStream))
            {
                for (var i = 0; i < tileManager.PageCount; i++)
                {
                    int pageWidth = 0, pageHeight = 0;
                    tileManager.LoadPage(i, ref pageWidth, ref pageHeight, false);
                    var tile = new Tile
                    {
                        PageNum = i,
                        Height = pageHeight,
                        Width = pageWidth,
                        Scale = 1.0
                    };
                    var page = tileManager.GetPageInBytes(tile, false);
                    result.Add(page);
                }
            }
            return result;
        }
    }
}
