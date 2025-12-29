using CloudinaryDotNet.Actions;

namespace BackendPortafolio.Helpers;

public interface IImagenServicio
{
    Task<ImageUploadResult> SubirImagenAsync(IFormFile archivo);
    Task<DeletionResult> EliminarImagenAsync(string publicId);
}