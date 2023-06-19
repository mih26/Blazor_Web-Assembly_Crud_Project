using Blazor_App_381.Shared.Models;
using Blazor_App_381.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blazor_App_381.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceDbContext db;
        private readonly IWebHostEnvironment env;
        public DevicesController(DeviceDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
        {
            return await db.Devices.ToListAsync();
        }
        [HttpGet("Specs")]
        public async Task<ActionResult<IEnumerable<Device>>> GetDeviceWithSpecs()
        {
            return await db.Devices.Include(x => x.Specs).ToListAsync();
        }
        [HttpGet("Specs/{id}")]
        public async Task<ActionResult<Device>> GetDeviceWithSpec(int id)
        {
            var d = await db.Devices.Include(x => x.Specs).FirstOrDefaultAsync(x => x.DeviceId == id);
            if (d == null) return NotFound();
            else return d;
        }
        [HttpPost]
        public async Task<ActionResult<Device>> PostDevice(Device device)
        {
            await db.Devices.AddAsync(device);
            await db.SaveChangesAsync();
            return device;
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Device>> PutDevice(int id, Device device)
        {
            if (id != device.DeviceId) return BadRequest("Id mismatch");
            var existing = await db.Devices.Include(x => x.Specs).FirstOrDefaultAsync(x => x.DeviceId == id);
            if (existing == null) return NotFound();
            existing.DeviceName = device.DeviceName;
            existing.ReleaseDate = device.ReleaseDate;
            existing.Price = device.Price;
            existing.OnSale = device.OnSale;
            existing.Picture = device.Picture;
            db.Specs.RemoveRange(existing.Specs);
            foreach (var s in device.Specs)
            {
                existing.Specs.Add(new Spec { SpecName = s.SpecName, Value = s.Value });
            }
            await db.SaveChangesAsync();
            return device;
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDevice(int id)
        {

            var existing = await db.Devices.Include(x => x.Specs).FirstOrDefaultAsync(x => x.DeviceId == id);
            if (existing == null) return NotFound();
            db.Devices.Remove(existing);
            await db.SaveChangesAsync();
            return NoContent();
        }
        [HttpPost("Upload")]
        public async Task<ImageUploadResponse> Upload(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName);
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var storedFileName = randomFileName + ext;
            using FileStream fs = new FileStream(Path.Combine(env.WebRootPath, "Pictures", storedFileName), FileMode.Create);
            await file.CopyToAsync(fs);
            fs.Close();
            return new ImageUploadResponse { FileName = file.FileName, StoredFileName = storedFileName };
        }
    }
}
