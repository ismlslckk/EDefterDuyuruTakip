﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDefterDuyuruTakip.Modeller
{
    public class Duyuru
    {
        public DateTime Tarih { get; set; }
        public string Icerik { get; set; }
        public bool Gonderildi { get; set; }
    }
}
