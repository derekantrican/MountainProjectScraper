﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MountainProjectDBBuilder
{
    public class AreaStats
    {
        #region Public Properties
        public int BouderRoutes { get; set; }
        public int TopRopeRoutes { get; set; }
        public int SportRoutes { get; set; }
        public int TradRoutes { get; set; }
        #endregion Public Properties

        public AreaStats()
        {

        }

        public AreaStats(int numBoulder, int numTR, int numSport, int numTrad)
        {
            this.BouderRoutes = numBoulder;
            this.TopRopeRoutes = numTR;
            this.SportRoutes = numSport;
            this.TradRoutes = numTrad;
        }

        public override string ToString()
        {
            List<string> resultParts = new List<string>();
            resultParts.Add(BouderRoutes > 0 ? "Boulder (" + BouderRoutes.ToString() + ")" : "");
            resultParts.Add(TopRopeRoutes > 0 ? "TR (" + TopRopeRoutes.ToString() + ")" : "");
            resultParts.Add(SportRoutes > 0 ? "Sport (" + SportRoutes.ToString() + ")" : "");
            resultParts.Add(TradRoutes > 0 ? "Trad (" + TradRoutes.ToString() + ")" : "");

            return string.Join(", ", resultParts.Where(s => !string.IsNullOrEmpty(s)));
        }
    }
}
