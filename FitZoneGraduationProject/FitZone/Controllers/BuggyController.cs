using FitZone.Repository.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitZone.APIs.Controllers
{

    public class BuggyController : BaseApiController
    {
        private readonly FitContext _dbcontext;

        public BuggyController(FitContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        [HttpGet("not-found")]
        public ActionResult GetNotFound() 
            => NotFound(); // 404


        [HttpGet("bad-request")]
        public ActionResult GetBadRequest()  
            => BadRequest(); // 400


        [HttpGet("server-error")]
        public ActionResult GetServerError()
            => Ok(_dbcontext.Users.Find(6000).ToString());




        [HttpGet("bad-request/{id}")] // Make Class To Solve it manually [validation error]
        public ActionResult GetBadRequestValidation(int id)
            => Ok(_dbcontext.Users.Find(id));

    }
}
