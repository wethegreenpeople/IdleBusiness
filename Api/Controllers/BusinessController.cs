﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdleBusiness.Data;
using IdleBusiness.Helpers;
using IdleBusiness.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdleBusiness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly BusinessHelper _businessHelper;
        private readonly EntrepreneurHelper _entrepreneurHelper;

        public BusinessController(ApplicationDbContext context, ILogger<BusinessController> logger)
        {
            _context = context;
            _logger = logger;
            _businessHelper = new BusinessHelper(_context, _logger);
            _entrepreneurHelper = new EntrepreneurHelper(_context, _logger);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{businessId}", Name = "Get")]
        public async Task<IActionResult> GetBusiness(string businessId)
        {
            _logger.LogTrace($"Get business {businessId}");
            var business = await _context.Business.SingleOrDefaultAsync(s => s.Id == Convert.ToInt32(businessId));

            return Ok(JsonConvert.SerializeObject(business, new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("/api/business/update")]
        public async Task<IActionResult> UpdateBusinessGains(string businessId)
        {
            async Task<Business> Update()
            {
                var business = await _businessHelper.UpdateGainsSinceLastCheckIn(Convert.ToInt32(businessId));
                business.Owner = await _entrepreneurHelper.UpdateEntrepreneurScore(Convert.ToInt32(businessId));
                return business;
            }
            try
            {
                var business = await Update();
                return Ok(JsonConvert.SerializeObject(business, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            catch { return StatusCode(500); }
            
            
        }
    }
}
