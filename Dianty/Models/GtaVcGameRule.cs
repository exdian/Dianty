using GameMonitor;
using System;
using System.Diagnostics;
using System.Linq;
using static Dianty.Models.GameManager;
using static GameMonitor.GtaVcMonitor;

namespace Dianty.Models;

public class GtaVcGameRule : GameRule
{
    public GtaVcGameRule(IMemoryService memoryService)
    {
        _monitor = new GtaVcMonitor(memoryService);
        _stopwatch = Stopwatch.StartNew();
        _monitor.PlayerTookDamage += Monitor_PlayerTookDamage;
        _monitor.PlayerBusted += Monitor_PlayerBusted;
        _monitor.PlayerWasted += Monitor_PlayerWasted;
        _monitor.PlayerWantedLevelChanged += Monitor_PlayerWantedLevelChanged;
        _monitor.PlayerFellOffBike += Monitor_PlayerFellOffBike;
    }

    ~GtaVcGameRule()
    {
        _monitor.Stop();
        _stopwatch.Stop();
    }

    private readonly GtaVcMonitor _monitor;
    private readonly Stopwatch _stopwatch;
    private float _playerTookDamageTotal;
    private long _damageRuleStart;

    public override bool IsEnable
    {
        get;
        set
        {
            if (value)
                _monitor.Start();
            else
                _monitor.Stop();
            field = value;
        }
    }

    public bool DamageRuleEnable
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                _playerTookDamageTotal = 0;
                DamageRuleOutputStrength = 0;
                ComputeOutputStrength();
            }
        }
    }

    public int DamageRuleThreshold
    {
        get;
        set
        {
            field = value;
            ComputePlayerTookDamageOutputStrength();
        }
    }

    public int DamageRuleStrength { get; set; }

    public int DamageRuleDuration { get; set; }

    public int DamageRuleOutputStrength
    {
        get
        {
            var now = _stopwatch.ElapsedTicks;
            var duration = DamageRuleDuration * TimeSpan.TicksPerSecond;
            if (now >= _damageRuleStart + duration)
                field = 0;
            return field;
        }

        set
        {
            field = value;
            _damageRuleStart = _stopwatch.ElapsedTicks;
        }
    }

    private void Monitor_PlayerTookDamage(object? sender, PlayerTookDamageEventArgs e)
    {
        if (!DamageRuleEnable)
            return;

        var damage = e.Damage;
        if (DamageRuleThreshold > 0 && damage > 0)
        {
            if (_playerTookDamageTotal < 0)
            {
                _playerTookDamageTotal = 0;
            }
        }
        else if (DamageRuleThreshold < 0 && damage < 0)
        {
            if (_playerTookDamageTotal > 0)
            {
                _playerTookDamageTotal = 0;
            }
        }
        else
        {
            return;
        }
        _playerTookDamageTotal = _playerTookDamageTotal + damage;
        ComputePlayerTookDamageOutputStrength();
    }

    private void Monitor_PlayerBusted(object? sender, PlayerBustedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerWasted(object? sender, PlayerWastedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerWantedLevelChanged(object? sender, PlayerWantedLevelChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void Monitor_PlayerFellOffBike(object? sender, PlayerFellOffBikeEventArgs e)
    {
        throw new NotImplementedException();
    }

    protected override int GetMaxStrength()
    {
        int[] strengths =
        [
            0, DamageRuleOutputStrength
        ];
        return strengths.Max();
    }

    protected override int SumStrength()
    {
        int result = 0;
        int[] strengths =
        [
            DamageRuleOutputStrength
        ];
        for (var i = 0; i < strengths.Length; i++)
        {
            var strength = strengths[i];
            if (strength > 0)
            {
                result = result + strength;
            }
        }
        return result;
    }

    private void ComputePlayerTookDamageOutputStrength()
    {
        if (!IsEnable || !DamageRuleEnable)
        {
            DamageRuleOutputStrength = 0;
        }
        else if ((DamageRuleThreshold > 0 && _playerTookDamageTotal > DamageRuleThreshold)
            || (DamageRuleThreshold < 0 && _playerTookDamageTotal < DamageRuleThreshold))
        {
            var multiple = _playerTookDamageTotal / DamageRuleThreshold;
            _playerTookDamageTotal %= DamageRuleThreshold;
            DamageRuleOutputStrength = DamageRuleOutputStrength + (int)(multiple * DamageRuleStrength);
        }
        else
        {
            return;
        }
        ComputeOutputStrength();
    }
}
