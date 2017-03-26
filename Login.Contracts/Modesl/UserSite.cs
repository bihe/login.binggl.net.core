using System;
using System.Collections.Generic;
using System.Text;

namespace Login.Contracts.Models
{
    public class UserSite : BaseModel
    {
        const string ListDeliminator = ";";

        public UserSite()
        {
            this.Permissions = new List<string>();
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Permissions
        {
            get
            {
                var splitString = PermissionList.Split(new string[] { ListDeliminator }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<string>();

                foreach (var s in splitString)
                {
                    list.Add(s);
                }
                return list;
            }
            set
            {
                PermissionList = string.Join(ListDeliminator, value);
            }
        }

        public string PermissionList { get; set; }

        public byte[] Timestamp { get; set; }

        public string UserEmail { get; set; }
        public User User { get; set; }
    }
}
