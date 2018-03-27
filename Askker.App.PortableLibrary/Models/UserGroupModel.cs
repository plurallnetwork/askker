using Newtonsoft.Json;
using System.Collections.Generic;

namespace Askker.App.PortableLibrary.Models
{    
    public class UserGroupModel
    {
        public string userId { get; set; }

        public string creationDate { get; set; }

        public string searchName { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public string profilePicture { get; set; }

        [JsonIgnore]
        public List<UserGroupMemberModel> members { get; set; }
                
        public UserGroupModel(string userId, string creationDate, string name, string profilePicture)
        {
            this.userId = userId;
            this.creationDate = creationDate;
            this.name = name;
            this.profilePicture = profilePicture;
        }

        public UserGroupModel(string userId, string creationDate, string name, string profilePicture, List<UserGroupMemberModel> members)
        {
            this.userId = userId;
            this.creationDate = creationDate;
            this.name = name;
            this.profilePicture = profilePicture;
            this.members = members;
        }

        [JsonConstructor]
        public UserGroupModel(string userId, string creationDate, string name, string searchName, string description, string profilePicture)
        {
            this.userId = userId;
            this.creationDate = creationDate;
            this.name = name;
            this.searchName = searchName;
            this.description = description;
            this.profilePicture = profilePicture;
        }
    }
}
