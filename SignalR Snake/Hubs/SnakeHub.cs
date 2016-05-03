﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.UI;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR_Snake.Models;
using Timer = System.Timers.Timer;

namespace SignalR_Snake.Hubs
{
    public class SnakeHub : Hub
    {
        public static List<Snake> Sneks = new List<Snake>();
        public static List<Food> Foods = new List<Food>();
        private static IHubCallerConnectionContext<dynamic> clientsStatic;
        public static Random Rng = new Random();

        public void NewSnek(string name)
        {
            Rng = new Random();
            Point start = new Point(Rng.Next(300, 700), Rng.Next(300, 700));
            string color = RandomColor();
            List<SnekPart> pos = new List<SnekPart>()
            {
                new SnekPart() {Color = color, Position = start, Name = name},
                new SnekPart() {Color = color, Position = new Point(start.X - 6, start.Y - 6),},
                new SnekPart() {Color = color, Position = new Point(start.X - 12, start.Y - 12)},
                new SnekPart() {Color = color, Position = new Point(start.X - 18, start.Y - 18)},
                new SnekPart() {Color = color, Position = new Point(start.X - 24, start.Y - 24)},
                new SnekPart() {Color = color, Position = new Point(start.X - 30, start.Y - 30)},
                new SnekPart() {Color = color, Position = new Point(start.X - 36, start.Y - 36)},
                new SnekPart() {Color = color, Position = new Point(start.X - 42, start.Y - 42)},
                new SnekPart() {Color = color, Position = new Point(start.X - 48, start.Y - 48)},
                new SnekPart() {Color = color, Position = new Point(start.X - 54, start.Y - 54)},
                new SnekPart() {Color = color, Position = new Point(start.X - 60, start.Y - 60)},
                new SnekPart() {Color = color, Position = new Point(start.X - 66, start.Y - 66)},
                new SnekPart() {Color = color, Position = new Point(start.X - 72, start.Y - 72)},
                new SnekPart() {Color = color, Position = new Point(start.X - 78, start.Y - 78)},
            };
            lock (Sneks)
            {
                Sneks.Add(new Snake()
                {
                    Name = name,
                    ConnectionId = Context.ConnectionId,
                    Direction = 0,
                    Parts = pos,
                    Width = 5,
                    Color = RandomColor()
                });
            }
            clientsStatic = Clients;
        }


        static SnakeHub()
        {
            Timer timer = new Timer(5) {AutoReset = true, Enabled = true};
            timer.Elapsed += Timer_Elapsed;

            Timer moveTimer = new Timer(5) { AutoReset = true, Enabled = true };
            moveTimer.Elapsed += MoveTimer_Elapsed;
        }

        private static void MoveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (Sneks)
            {
                foreach (var snek in Sneks)
                {
                    Point nextPosition;
                    if (snek.Fast)
                    {
                        nextPosition =
                            new Point(snek.Parts[0].Position.X + (int)(Math.Cos(snek.Dir * (Math.PI / 180)) * snek.SpeedTwo),
                                snek.Parts[0].Position.Y + (int)(Math.Sin(snek.Dir * (Math.PI / 180)) * snek.SpeedTwo));
                    }
                    else
                    {
                        nextPosition = new Point(snek.Parts[0].Position.X + (int)(Math.Cos(snek.Dir * (Math.PI / 180)) * snek.Speed),
                            snek.Parts[0].Position.Y + (int)(Math.Sin(snek.Dir * (Math.PI / 180)) * snek.Speed));
                    }


                    for (int i = 0; i < snek.Parts.Count - 1; i++)
                    {
                        if (i != snek.Parts.Count - 1)
                        {
                            snek.Parts[snek.Parts.Count - (i + 1)].Position =
                                snek.Parts[snek.Parts.Count - (2 + i)].Position;
                        }
                    }
                    snek.Parts[0].Position = nextPosition;
                    lock (Foods)
                    {
                        if (Foods.Count < 1000)
                        {
                            for (int i = 0; i < 1000 - Foods.Count; i++)
                            {
                                Point foodP = new Point(Rng.Next(0, 2000), Rng.Next(0, 2000));
                                Food food = new Food() { Color = RandomColor(), Position = foodP };
                                Foods.Add(food);
                                //Foods.Add(new Point(rng.Next(0,1000),rng.Next(0,1000)));
                            }
                        }

                        List<Food> toRemove = new List<Food>();
                        foreach (var food in Foods.ToList())
                        {
                            int w = snek.Width;
                            if (snek.Parts[0].Position.X - w <= food.Position.X &&
                                snek.Parts[0].Position.X + w >= food.Position.X &&
                                snek.Parts[0].Position.Y - w < food.Position.Y &&
                                snek.Parts[0].Position.Y + 2 * w > food.Position.Y)
                            {
                                snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                                snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                                snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                                toRemove.Add(food);
                            }
                        }
                        foreach (var food in toRemove)
                        {
                            Foods.Remove(food);
                        }
                    }
                }

            }
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (Sneks)
            {
                List<Snake> toRemoveSnakes = new List<Snake>();
                foreach (var snek in Sneks)
                {
                    if (!Sneks.Any(x => x.Parts.Any(c => c.Position.X > snek.Parts[0].Position.X - snek.Width &&
                                                         c.Position.X < snek.Parts[0].Position.X + snek.Width
                                                         && c.Position.Y > snek.Parts[0].Position.Y - snek.Width
                                                         && c.Position.Y < snek.Parts[0].Position.Y + snek.Width) &&
                                        snek != x)) return;

                    clientsStatic.User(snek.ConnectionId).Died();
                    foreach (var part in snek.Parts)
                    {
                        Foods.Add(new Food() {Color = RandomColor(), Position = part.Position});
                    }
                    toRemoveSnakes.Add(snek);
                    break;
                }
                foreach (var snek in toRemoveSnakes)
                {
                    Sneks.Remove(snek);
                }
            }
        }

        public void AllPos()
        {
            List<SnekPart> snakeParts = new List<SnekPart>();
            Point myPoint = new Point(0, 0);
            lock (Sneks)
            {
                foreach (var snek in Sneks)
                {
                    if (snek.ConnectionId == Context.ConnectionId)
                    {
                        myPoint = snek.Parts[0].Position;
                    }
                    snakeParts.AddRange(snek.Parts);
                }
                snakeParts.Reverse();
                Clients.Caller.AllPos(snakeParts, myPoint, Foods);
                //Clients.All.AllPos(snakePoints.ToArray(), myPoint);
            }
        }

        //public void MyPos()
        //{
        //    Point myPoint = Sneks.First(x => x.ConnectionId.Equals(Context.ConnectionId)).Position[0];
        //    Clients.Client(Context.ConnectionId).MyPos(myPoint);
        //}
        public void Speed()
        {
            lock (Sneks)
            {
                if (!Sneks.Any(x => x.ConnectionId.Equals(Context.ConnectionId))) return;
                    Snake snek = Sneks.First(x => x.ConnectionId.Equals(Context.ConnectionId));
                    snek.Fast = !snek.Fast;
            }
        }

        public void Score()
        {
            lock (Sneks)
            {
                List<SnekScore> snekScores =
                    Sneks.Select(snek => new SnekScore() {SnakeName = snek.Name, Length = snek.Parts.Count}).ToList();
                var ordered = snekScores.OrderByDescending(x => x.Length);
                Clients.Caller.Score(ordered);
            }
        }

        public static string RandomColor()
        {
            return $"#{Rng.Next(0x1000000):X6}";
        }

        //not being used?
        public void NewFood()
        {
            lock (Foods)
            {
                for (int i = 0; i < 250; i++)
                {
                    Point foodP = new Point(Rng.Next(0, 2000), Rng.Next(0, 2000));
                    string color = RandomColor();
                    Food food = new Food() {Color = color, Position = foodP};
                    Foods.Add(food);
                    Clients.All.Food(food);
                }
            }
        }

        public void CheckCollisions()
        {
            //
        }

        public void SendDir(double dir)
        {
            lock (Sneks)
            {
                foreach (var snek in Sneks.Where(snek => snek.ConnectionId.Equals(Context.ConnectionId)))
                {
                    snek.Dir = dir;
                }
            }

            #region 

/*
            lock (Sneks)
            {
                if (!Sneks.Any(x => x.ConnectionId.Equals(Context.ConnectionId))) return;
                Snake snek = Sneks.First(x => x.ConnectionId.Equals(Context.ConnectionId));
                Point nextPosition;
                if (snek.Fast)
                {
                    nextPosition =
                        new Point(snek.Parts[0].Position.X + (int) (Math.Cos(dir*(Math.PI/180))*snek.SpeedTwo),
                            snek.Parts[0].Position.Y + (int) (Math.Sin(dir*(Math.PI/180))*snek.SpeedTwo));
                }
                else
                {
                    nextPosition = new Point(snek.Parts[0].Position.X + (int) (Math.Cos(dir*(Math.PI/180))*snek.Speed),
                        snek.Parts[0].Position.Y + (int) (Math.Sin(dir*(Math.PI/180))*snek.Speed));
                }


                for (int i = 0; i < snek.Parts.Count - 1; i++)
                {
                    if (i != snek.Parts.Count - 1)
                    {
                        snek.Parts[snek.Parts.Count - (i + 1)].Position =
                            snek.Parts[snek.Parts.Count - (2 + i)].Position;
                    }
                }
                snek.Parts[0].Position = nextPosition;
                //snek.Position[0].X += (int)(Math.Cos(dir * (Math.PI / 180)) * snek.Speed);
                //snek.Position[0].Y += (int)(Math.Sin(dir * (Math.PI / 180)) * snek.Speed);
                /*

            List<Point> snakePoints = new List<Point>();

            foreach (var snake in Sneks)
            {
                for (int i = 0; i < snake.Position.Count; i++)
                {
                    snakePoints.Add(snake.Position[i]);
                }
            }
            
                lock (Foods)
                {
                    if (Foods.Count < 1000)
                    {
                        for (int i = 0; i < 1000 - Foods.Count; i++)
                        {
                            Point foodP = new Point(Rng.Next(0, 2000), Rng.Next(0, 2000));
                            Food food = new Food() {Color = RandomColor(), Position = foodP};
                            Foods.Add(food);
                            //Foods.Add(new Point(rng.Next(0,1000),rng.Next(0,1000)));
                        }
                    }

                    List<Food> toRemove = new List<Food>();
                    foreach (var food in Foods.ToList())
                    {
                        int w = snek.Width;
                        if (snek.Parts[0].Position.X - w <= food.Position.X &&
                            snek.Parts[0].Position.X + w >= food.Position.X &&
                            snek.Parts[0].Position.Y - w < food.Position.Y &&
                            snek.Parts[0].Position.Y + 2*w > food.Position.Y)
                        {
                            snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                            snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                            snek.Parts.Add(snek.Parts[snek.Parts.Count - 1]);
                            toRemove.Add(food);
                        }
                    }
                    foreach (var food in toRemove)
                    {
                        Foods.Remove(food);
                    }
                }
                */
            //Clients.Client(Context.ConnectionId).Pos(snakePoints.ToArray(), snek.Position[0]);}

            #endregion
        }
    }
}