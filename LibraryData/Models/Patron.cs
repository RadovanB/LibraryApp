using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData.Models
{
   public class Patron
    {
        public int Id { get; set; }
        public string FirstaName { get; set; }
        public  string LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TelehoneNumber { get; set; }

        public virtual LibraryCard LibraryCard { get; set; }
        public virtual LibraryBranch  HomeLibraryBranch { get; set; }
    }
}
