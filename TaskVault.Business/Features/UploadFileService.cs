using Firebase.Auth;
using Firebase.Storage;
using MsaCookingApp.Contracts.Features;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MsaCookingApp.DataAccess.Entities;
using MsaCookingApp.DataAccess.Repositories.Abstractions;

namespace MsaCookingApp.Business.Features
{
    public class UploadFileService : IUploadFileService
    {
        private static readonly string ApiKey = "AIzaSyD33LobThU7TsLDZncukl2SPLGjYyPLyN4";
        private static readonly string Bucket = "uploadfiledemo-768c1.firebasestorage.app";
        private static readonly string AuthEmail = "balanescu.alin03@gmail.com";
        private static readonly string AuthPassword = "Snoopy03022003";

        private readonly IRepository<UploadedFile> _uploadedFileRepository;
        
        public async Task<string> Upload(Stream stream, string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var authResult = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            
            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(authResult.FirebaseToken),
                        ThrowOnCancel = true
                    })
                .Child("uploads")
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);

            var t = await task;
            return t;
        }

        public async Task<string> GetFileUrl(string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var authResult = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            return await new FirebaseStorage(Bucket)
                .Child("uploads")
                .Child(fileName)
                .GetDownloadUrlAsync();
        }
    }
}