﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace SignalR_Snake.Models
{
    public class Snake
    {
        [DisplayName("Snake name")]
        [Required]
        public string Name { get; set; }
        public int Width { get; set; } = 10;
        public bool Fast { get; set; } = false;
        public int Speed { get; set; } = 4;
        public double Dir { get; set; } = 5;
        public int SpeedTwo { get; set; } = 8;
        public string ConnectionId { get; set; }
        public double Direction { get; set; }
        public List<SnekPart> Parts { get; set; }
        public string Color { get; set; }
    }
}