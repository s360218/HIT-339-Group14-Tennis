using Microsoft.AspNetCore.Identity;
using tennis.Models;

namespace tennis.Areas.Identity.Data;

public class tennisUser : IdentityUser
{
    public virtual Member? Member { get; set; }
    public virtual Coach? Coach { get; set; }
}
