using DAL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.Models
{
    public class ManageFallsModel
    {
        public static List<DAL.Fall> Falls { get; set; }
        public DAL.Model1 db = new Model1();

        public ManageFallsModel()
        {
            Falls = (from fall in db.falls
                     orderby fall.date
                     select fall).ToList();
        }

        public void UpdateFall(int id, float NewX, float NewY)
        {
            Falls = (from fall in db.falls
                     orderby fall.date
                     select fall).ToList();
            var Fall = (from f in Falls
                          where f.id == id
                          select f).FirstOrDefault<Fall>();

            if (Fall != null)
            {
                db = new Model1();
                db.Database.ExecuteSqlCommand("begin transaction\nUPDATE" +
                    "[dbo].[falls]\n\tSET [x] = " + NewX +
                    "\n\t\t,[y] = " + NewY +
                    "\n\t\t,[isGeotagged] = 1\n\t\t,[actDist] = " +
                    calculateActDist(Fall, NewX, NewY).ToString() +
                    "\nWHERE id = " + id + ";\ncommit;");
                Falls.Remove(Fall);
                Fall.x = NewX;
                Fall.y = NewY;
                Fall.isGeotagged = true;
                Fall.actDist = calculateActDist(Fall, NewX, NewY);
            }
        }

        public void Remove(int oldId)
        {
            Falls = (from fall in db.falls
                     orderby fall.date
                     select fall).ToList();
            var Fall = (from f in Falls
                          where f.id == oldId
                          select f).FirstOrDefault<Fall>();
            if (Fall != null)
            {
                db = new Model1();
                db.Database.ExecuteSqlCommand("begin transaction\nDELETE FROM[dbo].[falls]\n\tWHERE id = "+ oldId +";\ncommit;");
                Falls.Remove(Fall);
            }
        }

        #region Helper Methods
        private double calculateActDist(Fall fall, float NewX, float NewY)
        {
            // Haversine formula:
            //  a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
            //  c = 2 ⋅ atan2( √a, √(1−a) )
            //  d = R ⋅ c   <-- Spherical distance

            double oldX = fall.x, oldY = fall.y;

            var R = 6371e3; // Metres
            var φ1 = degreeToRadian(oldX);
            var φ2 = degreeToRadian(NewX);
            var Δφ = degreeToRadian(NewX - oldX);
            var Δλ = degreeToRadian(NewY - oldY);

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double dist = (R * c) / 1000; // Back to Km

            // Since we only really care about the first three digits
            // and to prevent overloadings:
            if (dist < 0.001)
            {
                dist = 0.0;
            }
            return dist;
        }
        private double degreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        #endregion
    }
}
