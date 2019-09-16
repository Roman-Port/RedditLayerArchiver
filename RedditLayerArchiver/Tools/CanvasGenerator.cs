using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditLayerArchiver.Responses.GQL;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedditLayerArchiver.Tools
{
    public static class CanvasGenerator
    {
        public const string SIDE_TEXT = "u/RomanPort";

        public static void GenerateAll(List<GqlResponse_Data_Subreddit_Layers_Edge> edges)
        {
            //Create the source image and fill it with white
            Image<Rgba32> image = new Image<Rgba32>(1920, 1080 + 100);
            for(int x = 0; x<image.Width; x++)
            {
                for(int y = 0; y<image.Height; y++)
                {
                    image[x, y] = new Rgba32(1f, 1f, 1f, 1f);
                }
            }

            //Load font
            FontCollection fonts = new FontCollection();
            FontFamily fontFam = fonts.Install("RobotoMono-Regular.ttf");
            var mainFont = fontFam.CreateFont(40);
            var mainFontRendererOptions = new RendererOptions(mainFont);
            var smallFont = fontFam.CreateFont(20);
            var smallFontRendererOptions = new RendererOptions(smallFont);

            //Start generation
            int saved = 0;
            int i = 0;
            Console.WriteLine("[Canvas] Starting generation...");
            List<Task> tasks = new List<Task>();
            foreach(var e in edges)
            {
                i++;
                
                //Skip if this edge does not have an image
                if (!File.Exists(Program.OUTPUT_IMGS + e.node.id + ".png"))
                    continue;

                //Add this edge to the image. First, open it
                try
                {
                    using (FileStream fs = new FileStream(Program.OUTPUT_IMGS + e.node.id + ".png", FileMode.Open))
                    using (Image<Rgba32> edgeImg = Image.Load<Rgba32>(fs))
                    {
                        //Resize the image
                        int sizeX = e.node.box.endPoint.x - e.node.box.startPoint.x;
                        int sizeY = e.node.box.endPoint.y - e.node.box.startPoint.y;
                        //edgeImg.Mutate(x => x.Resize(sizeX, sizeY));
                        edgeImg.Mutate(x => x.Resize(100, 100));

                        //Copy the image
                        for (int x = 0; x < edgeImg.Width; x++)
                        {
                            for (int y = 0; y < edgeImg.Height; y++)
                            {
                                //Mix color
                                Rgba32 sourcePixel = image[x + e.node.box.startPoint.x, y + e.node.box.startPoint.y];
                                Rgba32 edgePixel = edgeImg[x, y];
                                float edgeMult = ((float)edgePixel.A) / 255;
                                float sourceMult = 1 - edgeMult;
                                Rgba32 mixedColor = new Rgba32(
                                    ((((float)edgePixel.R) / 255) * edgeMult) + ((((float)sourcePixel.R) / 255) * sourceMult),
                                    ((((float)edgePixel.G) / 255) * edgeMult) + ((((float)sourcePixel.G) / 255) * sourceMult),
                                    ((((float)edgePixel.B) / 255) * edgeMult) + ((((float)sourcePixel.B) / 255) * sourceMult),
                                    ((((float)edgePixel.A) / 255) * edgeMult) + ((((float)sourcePixel.A) / 255) * sourceMult)
                                );
                                image[x + e.node.box.startPoint.x, y + e.node.box.startPoint.y] = mixedColor;
                            }
                        }
                    }
                }
                catch (UnknownImageFormatException err)
                {
                    //Move image to the corrupted folder
                    File.Move(Program.OUTPUT_IMGS + e.node.id + ".png", Program.OUTPUT_IMGS_CORRUPT + e.node.id + ".png");
                }
                catch (Exception ex)
                {
                    //Different error.
                    Console.Write("\r[Canvas] Failed to process image " + e.node.id + "! " + ex.Message + ex.StackTrace+"\n");
                }

                //Save every 6th image
                if (i % 8 == 0)
                {
                    //Clear bottom
                    for (int x = 0; x < image.Width; x++)
                    {
                        for (int y = 1080; y < 1080+100; y++)
                        {
                            image[x, y] = new Rgba32(0f, 0f, 0f, 1f);
                        }
                    }

                    //Load JSON data to get the date
                    var postData = JsonConvert.DeserializeObject<JArray>(File.ReadAllText(Program.OUTPUT_POSTS + e.node.id + ".json"));
                    double unixPostTime = (long)postData[0].SelectToken("data").SelectToken("children")[0].SelectToken("data").SelectToken("created_utc");
                    DateTime postTime = new DateTime(1970, 1, 1, 0, 0, 0).AddHours(-6).AddSeconds(unixPostTime);

                    //Create date string 
                    string date = $"{postTime.DayOfWeek.ToString()} {postTime.Month}/{postTime.Day}/{postTime.Year} {postTime.ToShortTimeString()} CST";

                    //Set date on the bottom
                    var textSize = TextMeasurer.Measure(date, mainFontRendererOptions);
                    float xTextPos = 50 - (textSize.Height / 2);
                    var pos = new SixLabors.Primitives.PointF(xTextPos, 1080 + 50 - (textSize.Height / 2));
                    image.Mutate(x => x.DrawText(date, mainFont, Rgba32.White, pos));

                    //Add credit on side
                    textSize = TextMeasurer.Measure(SIDE_TEXT, smallFontRendererOptions);
                    xTextPos = 1920 - xTextPos - textSize.Width;
                    pos = new SixLabors.Primitives.PointF(xTextPos, 1080 + 50 - (textSize.Height / 2));
                    image.Mutate(x => x.DrawText(SIDE_TEXT, smallFont, Rgba32.Gray, pos));

                    //Save
                    tasks.Add(SaveAsync(image.Clone(), Program.OUTPUT_CANVAS + saved++.ToString() + ".png"));
                }
                Console.Write($"\r[Canvas] Saved {i}/{edges.Count}...         ");
            }

            //Wait for all tasks to finish
            Console.Write("\r[Canvas] Waiting for tasks to finish...");
            Task.WaitAll(tasks.ToArray());

            //Save last image
            SaveAsync(image.Clone(), Program.OUTPUT_CANVAS + saved++.ToString() + ".png").GetAwaiter().GetResult();
        }

        private static async Task SaveAsync(Image<Rgba32> img, string pathname)
        {
            using (FileStream fs = new FileStream(pathname, FileMode.Create))
                img.SaveAsPng(fs);
        }
    }
}
