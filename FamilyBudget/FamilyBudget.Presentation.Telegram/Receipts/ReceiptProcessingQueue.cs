using System.Threading.Channels;

namespace FamilyBudget.Presentation.Telegram.Receipts;

public sealed class ReceiptProcessingQueue
{
    private const int Capacity = 10;

    private readonly Channel<ReceiptProcessingJob> _channel =
        Channel.CreateBounded<ReceiptProcessingJob>(new BoundedChannelOptions(Capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });

    public bool TryEnqueue(ReceiptProcessingJob job) =>
        _channel.Writer.TryWrite(job);

    public IAsyncEnumerable<ReceiptProcessingJob> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
