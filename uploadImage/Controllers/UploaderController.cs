using Microsoft.AspNetCore.Mvc;
using uploadImage.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Text;
using MimeKit.IO;
using MimeKit.Utils;
namespace uploadImage.Controllers {
        
    [Route("api/[controller]")]
    [ApiController]
    public class UploaderController:ControllerBase {
        private readonly Cloudinary cloudinary;

        public UploaderController() {
            var account = new Account(
                "dur3pty0r",
                "819371121139267",
                "sSGJ6ME-qjkc1zQRseJLrzWLSWI"     
            );
            cloudinary = new Cloudinary(account);
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadFile([FromForm] FileModels fileModel) {
            Response response = new Response();
            try {
                string path = Path.Combine(@"D:\Workspace\Test\uploadImage\myImages", fileModel.FileName);
                using (Stream stream = new FileStream(path, FileMode.Create)) {
                    fileModel.file.CopyTo(stream);
                }
                return Ok("Image created successfully");
            }
            catch (Exception ex) {
                return BadRequest();
            } 

           
        }
        [HttpPut("edit")]
        public IActionResult Edit([FromForm] FileModels fileModel) {
            var filePath = Path.Combine(@"D:\Workspace\Test\uploadImage\myImages",fileModel.FileName);

            if (!System.IO.File.Exists(filePath)) {
                return NotFound("File not found.");
            }

            // Đọc nội dung file cũ
            string fileContent = System.IO.File.ReadAllText(filePath);

            // Thực hiện chỉnh sửa nội dung theo yêu cầu của bạn, ví dụ:
            fileContent = $"New content: {fileContent}";

            // Ghi nội dung đã chỉnh sửa vào file
            System.IO.File.WriteAllText(filePath, fileContent);
            return Ok("File edited successfully.");
        }
        [HttpDelete("delete")]
        public IActionResult Delete([FromForm] string fileName) {
            var filePath = Path.Combine(@"D:\Workspace\Test\uploadImage\myImages", fileName);

            if (!System.IO.File.Exists(filePath)) {
                return NotFound("File not found.");
            }

            System.IO.File.Delete(filePath);

            return Ok("File deleted successfully.");
        }
        [HttpPost("UploadFileClouddinary")]
        public IActionResult UploadFileClouddinary() {

            var file = HttpContext.Request.Form.Files[0]; // Lấy tệp từ request

            if (file.Length > 0) {
                var uploadParams = new ImageUploadParams {
                    File = new FileDescription(file.FileName, file.OpenReadStream()), // Sử dụng stream của tệp
                };

                var uploadResult = cloudinary.Upload(uploadParams); // Thực hiện upload

                return Ok(uploadResult.SecureUri.AbsoluteUri); // Trả về URL của tệp đã upload
            }


            return BadRequest("No file was uploaded.");
        }
        [HttpPost("UploadFileDocxClouddinary")]
        public IActionResult UploadFileDocxClouddinary() {
            var file = HttpContext.Request.Form.Files[0]; // Lấy tệp từ request

            if (file.Length > 0) {
                var uploadParams = new RawUploadParams {
                    File = new FileDescription(file.FileName, file.OpenReadStream()), // Sử dụng stream của tệp
                };

                var uploadResult = cloudinary.Upload(uploadParams); // Thực hiện upload

                return Ok(uploadResult.SecureUri.AbsoluteUri); // Trả về URL của tệp đã upload
            }

            return BadRequest("No file was uploaded.");
        }
        [HttpPut("UpdateImage")]
        public async Task<IActionResult> UpdateImage([FromForm] string publicId) {
            var file = Request.Form.Files[0];
            if (file.Length == 0) {
                return BadRequest("No file was uploaded.");
            }

            // Xóa ảnh cũ trước khi tải lên ảnh mới
            var deleteParams = new DeletionParams(publicId);
            await cloudinary.DestroyAsync(deleteParams);

            var uploadParams = new ImageUploadParams {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = publicId,
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            return Ok(uploadResult.SecureUri.AbsoluteUri);
        }
        [HttpDelete("DeleteImage")]
        public async Task<IActionResult> DeleteImage([FromQuery] string publicId) {
            var deleteParams = new DeletionParams(publicId);
            var deleteResult = await cloudinary.DestroyAsync(deleteParams);
            if (deleteResult.Result == "ok") {
                return Ok("Image deleted successfully.");
            } else {
                return BadRequest("Failed to delete the image.");
            }
        }
        [HttpPost("SendEmail")]
        public IActionResult SendEmail([FromForm] string recipientEmail) {
            try {
                recipientEmail = "hiend4968@gmail.com";
               
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Данг Мань Хьен", "danghienxk@gmail.com"));
                    message.To.Add(new MailboxAddress("Кто-то", recipientEmail));
                    message.Subject = "Subject of the Email";

                    var multipart = new Multipart("mixed");

                    var text = new TextPart("plain") {
                        Text = "Ảnh gái Nga"
                    };

                    multipart.Add(text);

                    // Đường dẫn tới tệp ảnh đính kèm
                    var imagePath = "D:\\anh-gai-xinh-chau-au-my-1.jpg";
                    var attachment = new MimePart("application", "octet-stream") {
                        Content = new MimeContent(System.IO.File.OpenRead(imagePath), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(imagePath)
                    };
                    multipart.Add(attachment);
                    message.Body = multipart;
                   
                        using (var client = new SmtpClient()) {
                            client.Connect("smtp.gmail.com", 587, false);
                            client.Authenticate("danghienxk@gmail.com", "bmba mzez esss gmec");
                            client.Send(message);
                            client.Disconnect(true);
                        }
                    
                return Ok("Email sent successfully.");
            } catch (Exception ex) {
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

    }
}
