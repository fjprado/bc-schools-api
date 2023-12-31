﻿using bc_schools_api.Domain.Models.Entities;
using bc_schools_api.Domain.Models.Request;
using bc_schools_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bc_schools_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        public AddressController(IAddressService addressService) => _addressService = addressService;

        /// <summary>
        /// Search for address based on parameters
        /// </summary>
        /// <param name="addressRequest">Basis address to search for suggested addresses</param>
        /// <response code="200">Returns a address list</response>
        /// <response code="204">Returns if list is empty</response>   
        /// <response code="400">Returns if throws any exception</response>   
        [HttpPost("GetSuggestedAddressList")]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestedAddressList([FromBody] AddressRequest addressRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(addressRequest.Address))
                    throw new ArgumentException("Address needs to be informed");

                var addressList = await _addressService.GetSuggestedAddressList(addressRequest.Address);

                if (!addressList.Any())
                    return NoContent();

                return Ok(addressList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Search for coordinates based on typed address
        /// </summary>
        /// <param name="addressRequest">Basis address to search for coordinates</param>
        /// <response code="200">Returns a object with the coordinates</response>
        /// <response code="204">Returns if coordinate not found</response>   
        /// <response code="400">Returns if throws any exception</response> 
        [HttpPost("GetAddressCoordinate")]
        public async Task<ActionResult<Coordinate>> GetAddressCoordinate([FromBody] AddressRequest addressRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(addressRequest.Address))
                    throw new ArgumentException("Address needs to be informed"); 

                var coordenada = await _addressService.GetAddressCoordinate(addressRequest.Address);

                if (coordenada != null)
                    return Ok(coordenada);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
