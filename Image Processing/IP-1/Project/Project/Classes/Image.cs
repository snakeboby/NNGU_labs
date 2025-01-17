﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IP1.Imaging
{
    public class Image
    {
        private Color[,] data;

        public int Height => data.GetLength(0);
        public int Width => data.GetLength(1);

        public Color this[int indexY, int indexX]
        {
            get { return data[indexY, indexX]; }
            set {
                if (value == null)
                    throw new Exception("Color can't be null");
                data[indexY, indexX] = value;
                }
        }

        public Image(uint sizeX, uint sizeY)
        {
            data = new Color[sizeY, sizeX];
            for (int y = 0; y < data.GetLength(0); y++)
                for (int x = 0; x < data.GetLength(1); x++)
                    data[y, x] = Color.White;

        }

        public void Fill(Color color)
        {
            for (int y = 0; y < data.GetLength(0); y++)
                for (int x = 0; x < data.GetLength(1); x++)
                    data[y, x] = new Color(color);
        }

        public IEnumerable<byte> GetBytesBGR24()
        {
            for (int y = 0; y < data.GetLength(0); y++)
            {
                for (int x = 0; x < data.GetLength(1); x++)
                {
                    Color color = data[y, x];
                    yield return color.b;
                    yield return color.g;
                    yield return color.r;
                }
            }
        }

        public void Save(String savePath)
        {
            int maxValue = 255;
            int maxLineSize = 70;

            using (System.IO.StreamWriter streamWriter = System.IO.File.CreateText(savePath + ".ppm"))
            {
                streamWriter.WriteLine("P3");
                streamWriter.WriteLine(Width + " " + Height);
                streamWriter.WriteLine(maxValue);

                String line = "";

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        String pixel = data[y, x].r + " " + data[y, x].g + " " + data[y, x].b;
                        if (line.Length + pixel.Length + 1 > maxLineSize)
                        {
                            streamWriter.WriteLine(line);
                            line = "";
                        }
                        line += " " + pixel;
                    }
                }

                streamWriter.WriteLine(line);
                streamWriter.Close();
            }
        }

        public static Image Load(String savePath)
        {
            string magicNumber=null;
            int? maxValue = null;
            int? width = null;
            int? height = null;
            Image image = null;
            byte?[] pixel = new byte?[3] { null, null, null};
            int nextPixel = 0;

            using (System.IO.StreamReader streamReader = System.IO.File.OpenText(savePath + ".ppm"))
            {
                string line = streamReader.ReadLine();
                while(line!=null)
                {
                    line = line.Split(new char[] { '#' },2)[0];
                    string[] words = line.ToLower().Split(new char[] { ' ', '\n', '\t' });

                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i] == String.Empty)
                            continue;
                            
                        if(magicNumber==null)
                        {
                            if(words[i]!="p3")
                            {
                                throw new Exception("Magic number should be 'p3'");
                            }
                            magicNumber = words[i];
                        }
                        else
                        {
                            int value;
                            if(!Int32.TryParse(words[i], out value))
                            {
                                throw new Exception("Expected number, but got '" + words[i] + "'");
                            }

                            
                            if(width==null)
                            {
                                width = value;
                            }
                            else if(height==null)
                            {
                                height = value;
                                image = new Image((uint)width, (uint)height);
                            }
                            else if (maxValue == null)
                            {
                                maxValue = value;
                            }
                            else if(nextPixel>=width*height)
                            {
                                throw new Exception("Got more than need.");
                            }
                            else
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    if(pixel[j]==null)
                                    {
                                        pixel[j] = (byte)(255.0 * value / maxValue);
                                        break;
                                    }
                                }
                                if(pixel[2]!=null)
                                {
                                    int posX = (int)(nextPixel % width);
                                    int posY = (int)(nextPixel / width);
                                    image[posY, posX] = new Color((byte)pixel[0], (byte)pixel[1], (byte)pixel[2]);
                                    pixel = new byte?[3] { null, null, null };
                                    nextPixel++;
                                }
                            }
                        }
                    }
                    line = streamReader.ReadLine();
                }

                if(image==null || nextPixel!=width*height)
                {
                    throw new Exception("Got less than need.");
                }

                streamReader.Close();
            }

            return image;
        }

    }
}
