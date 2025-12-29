using BackendPortafolio.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BackendPortafolio.Services;

public class ImagenServicio : IImagenServicio
{
    private readonly Cloudinary _cloudinary;

    public ImagenServicio(IConfiguration config)
    {
        //Leer las credenciales desde el appsenttings
        var acc = new Account(
            config["CloudinarySettings:CloudName"],
            config["CloudinarySettings:ApiKey"],
            config["CloudinarySettings:ApiSecret"]
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> SubirImagenAsync(IFormFile archivo)
    {
        var uploadResult = new ImageUploadResult();

        if (archivo.Length > 0)
        {
            using var stream = archivo.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),

                //Transformación de la imagen (16:9)
                Transformation = new Transformation()
                    .Width(1280)
                    .Height(720)
                    .Crop("fill")
                    .Gravity("center")
                    .Quality("auto")
                    .FetchFormat("webp"),
                Folder = "proyectos_portafolio"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;

    }

    public async Task<DeletionResult> EliminarImagenAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        return await _cloudinary.DestroyAsync(deleteParams);
    }

}