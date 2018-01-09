namespace Askker.App.PortableLibrary.Models
{
    public class UserGroupMemberModel
    {
        public string id { get; set; }

        public string relationshipStatus { get; set; }

        public string requestDate { get; set; }

        public string name { get; set; }

        public string acceptDate { get; set; }

        public string profilePicture { get; set; }

        public UserGroupMemberModel(string id, string relationshipStatus, string requestDate, string profilePicture)
        {
            this.id = id;
            this.relationshipStatus = relationshipStatus;
            this.requestDate = requestDate;
            this.profilePicture = profilePicture;
        }

        public UserGroupMemberModel(string id, string relationshipStatus, string name, string requestDate, string acceptDate, string profilePicture)
        {
            this.id = id;
            this.relationshipStatus = relationshipStatus;
            this.name = name;
            this.requestDate = requestDate;
            this.acceptDate = acceptDate;
            this.profilePicture = profilePicture;
        }
    }
}
