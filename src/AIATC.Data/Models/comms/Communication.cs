using AIATC.Data.Models.Types;
namespace AIATC.Data.Models.Comms;

public abstract class Communication<TTransmitter> : Record424<TTransmitter> where TTransmitter : Transmitter
{
    public CommClass Class { get; set; }
}
