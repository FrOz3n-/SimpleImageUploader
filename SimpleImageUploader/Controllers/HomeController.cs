using Goheer.EXIF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace SimpleImageUploader.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var files = Directory.GetFiles(Server.MapPath("~/Content/images/")).ToList();

            List<string> imageFiles = new List<string>();
            foreach (string filename in files)
            {
                if (Regex.IsMatch(filename, @".jpg|.png|.gif$"))
                    imageFiles.Add(filename);
            }
            return View(imageFiles);
        }

        public ActionResult Upload()
        {
            return View();
        }

        public ActionResult WrongUpload(HttpPostedFileBase file)
        {
            string pic = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(Server.MapPath("~/Content/images/"), pic);
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap bitmap = new Bitmap(Image.FromStream(file.InputStream));
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Position = 0;
                byte[] image = new byte[stream.Length + 1];
                stream.Read(image, 0, image.Length);

                System.IO.File.WriteAllBytes(path, image);
            }

            return RedirectToAction("Index");
        }

        public ActionResult RightUpload(HttpPostedFileBase file)
        {
            string pic = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(Server.MapPath("~/Content/images/"), pic);
            using (MemoryStream ms = new MemoryStream())
            {
                var theImage = Image.FromStream(file.InputStream);
                Bitmap bitmap = new Bitmap(theImage);
                MemoryStream stream = new MemoryStream();
                foreach (var item in theImage.PropertyItems)
                    bitmap.SetPropertyItem(item);

                var exif = new EXIFextractor(ref bitmap, "n");
                RotateFlipType flip = OrientationToFlipType(exif["Orientation"] == null ? "0" : exif["Orientation"].ToString());

                if (flip != RotateFlipType.Rotate180FlipNone)
                {
                    foreach (var item in theImage.PropertyItems)
                        bitmap.SetPropertyItem(item);
                }

                bitmap.RotateFlip(flip);
                exif.setTag(0x112, "1"); // Optional: reset orientation tag

                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                stream.Position = 0;
                byte[] image = new byte[stream.Length + 1];
                stream.Read(image, 0, image.Length);

                System.IO.File.WriteAllBytes(path, image);
            }
            return RedirectToAction("Index");
        }

        // Match the orientation code to the correct rotation:

        private static RotateFlipType OrientationToFlipType(string orientation)
        {
            switch (int.Parse(orientation))
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }
    }
}