using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Plugin.Media;
using Plugin.Permissions;

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
            TakePhoto();
        }

        private async void TakePhoto()
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

            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
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
                _imageView.SetImageURI(uri);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}