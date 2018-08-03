using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MidnightLizard.Web.Identity.Models;
using Newtonsoft.Json;

namespace MidnightLizard.Web.Identity.Security.Claims
{
    public enum AppRole
    {
        Owner
    }
}