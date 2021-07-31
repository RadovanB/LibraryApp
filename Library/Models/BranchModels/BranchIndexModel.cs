using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.BranchModels
{
    public class BranchIndexModel
    {
        public IEnumerable<BranchDetailModel> Branches { get; set; }
    }
}
