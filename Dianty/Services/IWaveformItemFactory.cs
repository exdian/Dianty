using Dianty.ViewModels;
using DungeonToolkit.Coyote;
using System;

namespace Dianty.Services;

public interface IWaveformItemFactory
{
    WaveformItemBase CreateWaveformItem(IWaveformInfo waveformInfo, Func<ILocalizationService, string>? defaultNameGetter);
    WaveformDecoderItem CreateWaveformDecoderItem(IWaveformDecoderInfo waveformDecoderInfo, Channel channel);
}
