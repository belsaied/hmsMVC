using HMS.DAL.Models.WardBedModule;

namespace HMS.BLL.Services.Specifications.WardBedModule
{
    public sealed class WardByNameSpecification : BaseSpecifications<Ward, int>
    {
        public WardByNameSpecification(string name)
            : base(w => w.Name.ToLower() == name.ToLower())
        {
        }
    }
}
