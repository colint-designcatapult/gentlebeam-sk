using Com.Empyreanmed.Heracles.Photos.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

/// <summary>
/// SQLite-backed PhotosService. Binary photo data is stored as Base-64 in a
/// companion byte-blob table; metadata (Photo message) goes into the normal
/// JSON table.
/// </summary>
public sealed class PhotosServiceImpl : PhotosService.PhotosServiceBase
{
    // In-memory photo blobs keyed by photo id (sufficient for embedded server).
    private static readonly Dictionary<long, byte[]> _blobs = new();
    private static readonly object _blobLock = new();

    private readonly SqliteProtoRepository<Photo> _repo;
    public PhotosServiceImpl(SqliteProtoRepository<Photo> repo) => _repo = repo;

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public override async Task<ListPhotosResponse> ListPhotos(ListPhotosRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.DiagnosisId);
        var r = new ListPhotosResponse();
        r.Photos.AddRange(items);
        return r;
    }

    public override async Task<GetPhotoResponse> GetPhoto(GetPhotoRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.PhotoId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Photo {request.PhotoId} not found"));
        return new GetPhotoResponse { Photo = item };
    }

    public override async Task<CreatePhotoResponse> CreatePhoto(CreatePhotoRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Photo, request.Photo.DiagnosisId);
        return new CreatePhotoResponse { Photo = created };
    }

    public override async Task<UpdatePhotoResponse> UpdatePhoto(UpdatePhotoRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Photo.Id, request.Photo);
        return new UpdatePhotoResponse { Photo = updated };
    }

    public override async Task<DeletePhotoResponse> DeletePhoto(DeletePhotoRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.PhotoId);
        lock (_blobLock)
            _blobs.Remove(request.PhotoId);
        return new DeletePhotoResponse();
    }

    // ── binary streaming ─────────────────────────────────────────────────────

    public override async Task<SendPhotoResponse> SendPhoto(
        IAsyncStreamReader<SendPhotoRequest> requestStream,
        ServerCallContext context)
    {
        var chunks = new SortedDictionary<int, byte[]>();
        long photoId = 0;

        await foreach (var chunk in requestStream.ReadAllAsync(context.CancellationToken))
        {
            photoId = chunk.PhotoId;
            chunks[chunk.ChunkIndex] = chunk.ChunkData.ToByteArray();
        }

        if (photoId > 0)
        {
            var blob = chunks.Values.SelectMany(b => b).ToArray();
            lock (_blobLock)
                _blobs[photoId] = blob;
        }

        return new SendPhotoResponse { Message = "OK", TotalFilesReceived = chunks.Count };
    }

    public override async Task ReceivePhoto(
        ReceivePhotoRequest request,
        IServerStreamWriter<ReceivePhotoResponse> responseStream,
        ServerCallContext context)
    {
        byte[]? blob;
        lock (_blobLock)
            _blobs.TryGetValue(request.PhotoId, out blob);

        if (blob is null || blob.Length == 0)
            return;

        const int chunkSize = 65536;
        int total = (int)Math.Ceiling((double)blob.Length / chunkSize);

        for (int i = 0; i < total; i++)
        {
            int offset = i * chunkSize;
            int length = Math.Min(chunkSize, blob.Length - offset);
            var chunk = Google.Protobuf.ByteString.CopyFrom(blob, offset, length);

            await responseStream.WriteAsync(new ReceivePhotoResponse
            {
                ChunkData  = chunk,
                ChunkIndex = i,
                TotalChunks = total
            }, context.CancellationToken);
        }
    }
}
