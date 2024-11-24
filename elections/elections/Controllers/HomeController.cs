using elections.Models;
using System.Linq;
using System.Web.Mvc;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Net;

namespace elections.Controllers
{
    public class HomeController : Controller
    {
        public readonly ElectionEntities db = new ElectionEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(Feedback model)
        {

            if (ModelState.IsValid)
            {
                db.Feedbacks.Add(model);
                db.SaveChanges();

                ViewBag.Message = "تم إرسال رسالتك بنجاح!";
                return View();
            }

            return View(model);
        }


        public ActionResult Services()
        {
            ViewBag.Message = "Your serviec page.";

            return View();
        }

        [HttpGet]
        public ActionResult ResetPassword()
        {
            return View();
        }

        // POST: ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the national ID and email match
                var existingUser = db.Users.FirstOrDefault(u => u.national_id == user.national_id && u.email == user.email);

                if (existingUser != null)
                {
                    // Generate a reset token
                    string resetToken = GenerateResetToken();

                    // Send the reset token to the user's email
                    SendResetTokenEmail(existingUser.email, resetToken);

                    // Store the token and email temporarily (e.g., in TempData) and redirect to the confirm reset token view
                    TempData["UserEmail"] = existingUser.email;
                    TempData["ResetToken"] = resetToken;

                    return RedirectToAction("ConfirmResetToken");
                }
                else
                {
                    ModelState.AddModelError("", "الرقم الوطني والبريد الإلكتروني لا يتطابقان.");
                }
            }

            return View(user);
        }

        // GET: ConfirmResetToken
        [HttpGet]
        public ActionResult ConfirmResetToken()
        {
            return View();
        }

        // POST: ConfirmResetToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmResetToken(string email, string resetToken)
        {
            // Check if the token matches
            if (resetToken == TempData["ResetToken"]?.ToString() && email == TempData["UserEmail"]?.ToString())
            {
                return RedirectToAction("SetNewPassword");
            }
            else
            {
                ModelState.AddModelError("", "رمز إعادة تعيين كلمة المرور أو البريد الإلكتروني غير صحيح.");
            }

            return View();
        }

        // GET: SetNewPassword
        [HttpGet]
        public ActionResult SetNewPassword()
        {
            return View();
        }

        // POST: SetNewPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetNewPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword == confirmPassword)
            {
                // Find the user by email
                var user = db.Users.FirstOrDefault(u => u.email == email);
                if (user != null)
                {
                    // Update the user's password
                    user.password = newPassword;
                    db.SaveChanges();

                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "المستخدم غير موجود.");
                }
            }
            else
            {
                ModelState.AddModelError("", "كلمة المرور الجديدة وتأكيد كلمة المرور لا تتطابقان.");
            }

            return View();
        }

        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString().Substring(0, 8); // Example: generates a random 8-character string
        }

        private void SendResetTokenEmail(string toEmail, string resetToken)
        {
            
                var fromEmail = "techlearnhub.contact@gmail.com";
                var SmtpPassword = "lyrlogeztsxclank";
                string subjectText = "Your Confirmation Code";
                string messageText = $"Your confirmation code is {resetToken}";

                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("YourAppName", fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Your Password Reset Token";
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(fromEmail);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subjectText;
                mailMessage.Body = messageText;
                mailMessage.IsBodyHtml = false;

                using (System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(fromEmail, SmtpPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mailMessage);
                }
            }



        }


        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User user)
        {
            if (ModelState.IsValid)
            {
                // Authenticate the user
                var existingUser = db.Users.FirstOrDefault(u => u.email == user.email && u.password == user.password);
                if (existingUser != null)
                {
                    Session["UserEmail"] = existingUser.email;

                    // Redirect to a secure area or home page
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }
            return View(user);
        }

    }
}