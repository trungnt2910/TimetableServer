using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TimetableServer.Models;
using TimetableServer.Services;

namespace TimetableServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimetableController : ControllerBase
    {
        private readonly TimetableService _service;

        public TimetableController(TimetableService service)
        {
            _service = service;
        }

        [HttpGet("getLocation")]
        public ActionResult<TimetableLocationResponse> Get(string id, string password)
        {
            try
            {
                var domainName = $"{Request.Scheme}://{Request.Host}";

                var result = _service.Get(id, password, domainName);

                if (result == null)
                {
                    return NotFound();
                }

                return result;
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPut("update")]
        public ActionResult<bool> Update(string id, string updatePassword)
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var data = reader.ReadToEnd();

                if (_service.Update(id, updatePassword, data))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            return BadRequest();
        }

        [HttpGet("getTimetable")]
        public ActionResult<string> GetTimetable(string uuid)
        {
            try
            {
                var result = _service.GetTimetable(uuid);

                if (result == null)
                {
                    return NotFound();
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("create")]
        public ActionResult<string> CreateTimetable(string password, string updatePassword)
        {
            try
            {
                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;

                using var reader = new StreamReader(Request.Body);
                var data = reader.ReadToEnd();

                var result = _service.CreateTimetable(remoteIpAddress, password, updatePassword, data);
                if (result == null)
                {
                    return BadRequest();
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpDelete("delete")]
        public ActionResult<bool> DeleteTimetable(string id, string updatePassword)
        {
            try
            {
                if (_service.DeleteTimetable(id, updatePassword))
                {
                    return true;
                }
                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
