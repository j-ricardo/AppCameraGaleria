using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Plugin.Media;
using Plugin.Permissions;
using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Android;

namespace AppCameraGaleria
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private ImageView _imageView;
        public static readonly int PickImageId = 1000;

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            var btnCamera = (Button)FindViewById(Resource.Id.bCamera);
            var btnGaleria = (Button)FindViewById(Resource.Id.bGaleria);
            _imageView = (ImageView)FindViewById(Resource.Id.ivImage);
            btnCamera.Click += BtnCamera_Click;
            btnGaleria.Click += BtnGaleria_Click;
            RequestPermissions(permissionGroup, 0);
        }

        private void BtnCamera_Click(object sender, System.EventArgs e)
        {
            CapturarFoto();
        }

        private async void CapturarFoto()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Small,
                CompressionQuality = 80,
                Name = "myimage.jpg",
                Directory = "sample"

            });

            if (file == null)
                return;

            byte[] imageArray = File.ReadAllBytes(file.Path);
            if (imageArray.Length > 100000)
            {
                var novaImagem = imageArray;
                while (novaImagem.Length > 100000)
                {
                    var bit = BitmapFactory.DecodeByteArray(novaImagem, 0, novaImagem.Length);
                    var altReduzida = Convert.ToInt32(bit.Height * 0.9);
                    var largReduzida = Convert.ToInt32(bit.Width * 0.9);
                    /*
                    var stream = new MemoryStream();
                    bit.Compress(Bitmap.CompressFormat.Png, 0, stream);
                    novaImagem = stream.ToArray();*/
                }
                imageArray = novaImagem;
            }

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            _imageView.SetImageBitmap(bitmap);
        }

        private void BtnGaleria_Click(object sender, System.EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Selecione a foto"), PickImageId);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
                var imagemInBytes = ConvertImageToByte(uri);
                imagemInBytes = LoopReducaoImagem(imagemInBytes);
                var bitmap = BitmapFactory.DecodeByteArray(imagemInBytes, 0, imagemInBytes.Length);
                _imageView.SetImageBitmap(bitmap);
            }
        }



        private byte[] ConvertImageToByte(Android.Net.Uri uri)
        {
            Stream stream = ContentResolver.OpenInputStream(uri);
            byte[] byteArray;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }
            return byteArray;
        }

        private byte[] LoopReducaoImagem(byte[] imageArray)
        {
            var imagemEmBytes = imageArray;
            if (imagemEmBytes.Length > 100000)
            {
                var novaImagem = imageArray;
                while (novaImagem.Length > 120000)
                {
                    try
                    {
                        var bitmap = SKBitmap.Decode(novaImagem);
                        var proporcao = (bitmap.Width > 1900 || bitmap.Height > 1900) ? 0.5 : 0.8; //Proporção de redução
                        var altReduzida = Convert.ToInt32(bitmap.Height * proporcao);
                        var largReduzida = Convert.ToInt32(bitmap.Width * proporcao);
                        var novoBytes = ResizeImage(novaImagem, largReduzida, altReduzida);
                        novaImagem = novoBytes;
                    }

                    catch { return null; }                    
                }
                imagemEmBytes = novaImagem;
            }

            return imagemEmBytes;
        }

        public byte[] ResizeImage(byte[] imageData, int width, int height)
        {
            var bitmap = SKBitmap.Decode(imageData);
            SKImageInfo desired = new SKImageInfo(width, height);
            bitmap = bitmap.Resize(desired, SKFilterQuality.Medium);
            var st = new MemoryStream();
            bitmap.ToBitmap().Compress(Bitmap.CompressFormat.Png, 100, st);
            return st.ToArray();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}