using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.Models
{
    public class ManageUsersModel
    {
        public static List<DAL.user> Users { get; set; }
        public DAL.userContext db = new DAL.userContext();

        public ManageUsersModel()
        {
            Users = (from user in db.users
                     select user).ToList();
        }

        public void UpdateUser(string Username, string NewPhotoUrl)
        {
            Users = (from user in db.users
                     select user).ToList();
            var User = Users.Where(u => u.username.Trim() == Username).FirstOrDefault();
            if (User != null)
            {
                db = new DAL.userContext();
                db.Database.ExecuteSqlCommand(
                    "begin transaction\nUPDATE [dbo].[users]\n" +
                    "\tSET [photoUrl] = \'" + NewPhotoUrl +
                    "\'\n\tWHERE [username] = \'" + User.username.Trim() +
                    "\';\ncommit;");
                User.photoUrl = NewPhotoUrl;
            }
        }
    }
}
