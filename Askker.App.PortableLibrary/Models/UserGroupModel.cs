using Newtonsoft.Json;

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
                
        public UserGroupModel(string userId, string creationDate, string name, string profilePicture)
        {
            this.userId = userId;
            this.creationDate = creationDate;
            this.name = name;
            this.profilePicture = profilePicture;
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
