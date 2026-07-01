using CommunityToolkit.Mvvm.Messaging;
using Dianty.Utils;
using GameMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using static Dianty.Models.GameManager;
using static GameMonitor.GtaVcMonitor;

namespace Dianty.Models;

public class GtaVcGameRule : GameRule
{
    public GtaVcGameRule(IMemoryService memoryService)
    {
        _monitor = new GtaVcMonitor(memoryService);
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(RecoverStrength, null, Timeout.Infinite, Timeout.Infinite);

        _monitor.PlayerTookDamage += Monitor_PlayerTookDamage;
        _monitor.PlayerBusted += Monitor_PlayerBusted;
        _monitor.PlayerWasted += Monitor_PlayerWasted;
        _monitor.PlayerWantedLevelChanged += Monitor_PlayerWantedLevelChanged;
        _monitor.PlayerFellOffBike += Monitor_PlayerFellOffBike;

        _dueTime = new Dictionary<Rule, long>
        {
            [Rule.Damage] = 0,
            [Rule.Busted] = 0,
            [Rule.Wasted] = 0,
            [Rule.MiTang] = 0,
            [Rule.WantedLevel] = 0,
            [Rule.FellOffBike] = 0,
        };
    }

    ~GtaVcGameRule()
    {
        _monitor.Stop();
        _stopwatch.Stop();
        _timer.Dispose();

        _monitor.PlayerTookDamage -= Monitor_PlayerTookDamage;
        _monitor.PlayerBusted -= Monitor_PlayerBusted;
        _monitor.PlayerWasted -= Monitor_PlayerWasted;
        _monitor.PlayerWantedLevelChanged -= Monitor_PlayerWantedLevelChanged;
        _monitor.PlayerFellOffBike -= Monitor_PlayerFellOffBike;
    }

    private readonly GtaVcMonitor _monitor;
    private readonly Stopwatch _stopwatch;
    private readonly Timer _timer;
    private readonly Dictionary<Rule, long> _dueTime;
    private float _playerTookDamageTotal;
    private long _damageRuleStartTick;
    private long _bustedRuleStartTick;
    private long _wastedRuleStartTick;
    private long _miTangRuleStartTick;

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
            }
        }
    }

    public int DamageRuleThreshold
    {
        get;
        set
        {
            field = value;
            ComputeDamageRuleOutputStrength();
        }
    }

    public int DamageRuleStrength { get; set; }

    public int DamageRuleDuration
    {
        get;
        set
        {
            field = value;
            TryRecoverRuleOutputStrength(_damageRuleStartTick, value, Rule.Damage, s => DamageRuleOutputStrength = s);
            ChangeTimer();
        }
    }

    public int DamageRuleOutputStrength
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                if (value != 0)
                {
                    _damageRuleStartTick = _stopwatch.ElapsedTicks;
                    _dueTime[Rule.Damage] = DamageRuleDuration * TimeSpan.TicksPerSecond;
                    ChangeTimer();
                }
                ComputeOutputStrength();
            }
        }
    }

    public bool BustedRuleEnable
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                BustedRuleOutputStrength = 0;
            }
        }
    }

    public int BustedRuleStrength { get; set; }

    public int BustedRuleDuration
    {
        get;
        set
        {
            field = value;
            TryRecoverRuleOutputStrength(_bustedRuleStartTick, value, Rule.Busted, s => BustedRuleOutputStrength = s);
            ChangeTimer();
        }
    }

    public int BustedRuleOutputStrength
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                if (value != 0)
                {
                    _bustedRuleStartTick = _stopwatch.ElapsedTicks;
                    _dueTime[Rule.Busted] = BustedRuleDuration * TimeSpan.TicksPerSecond;
                    ChangeTimer();
                }
                ComputeOutputStrength();
            }
        }
    }

    public bool WastedRuleEnable
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                WastedRuleOutputStrength = 0;
            }
        }
    }

    public int WastedRuleStrength { get; set; }

    public int WastedRuleDuration
    {
        get;
        set
        {
            field = value;
            TryRecoverRuleOutputStrength(_wastedRuleStartTick, value, Rule.Wasted, s => WastedRuleOutputStrength = s);
            ChangeTimer();
        }
    }

    public int WastedRuleOutputStrength
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                if (value != 0)
                {
                    _wastedRuleStartTick = _stopwatch.ElapsedTicks;
                    _dueTime[Rule.Wasted] = WastedRuleDuration * TimeSpan.TicksPerSecond;
                    ChangeTimer();
                }
                ComputeOutputStrength();
            }
        }
    }

    public bool MiTangRuleEnable
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                MiTangRuleOutputStrength = 0;
            }
        }
    }

    public int MiTangRuleStrength { get; set; }

    public int MiTangRuleDuration
    {
        get;
        set
        {
            field = value;
            TryRecoverRuleOutputStrength(_miTangRuleStartTick, value, Rule.MiTang, s => MiTangRuleOutputStrength = s);
            ChangeTimer();
        }
    }

    public int MiTangRuleOutputStrength
    {
        get;
        private set
        {
            if (field != value)
            {
                field = value;
                if (value != 0)
                {
                    _miTangRuleStartTick = _stopwatch.ElapsedTicks;
                    _dueTime[Rule.MiTang] = MiTangRuleDuration * TimeSpan.TicksPerSecond;
                    ChangeTimer();
                }
                ComputeOutputStrength();
            }
        }
    }

    private void RecoverStrength(object? state)
    {
        TryRecoverRuleOutputStrength(_damageRuleStartTick, DamageRuleDuration, Rule.Damage, s => DamageRuleOutputStrength = s);
        TryRecoverRuleOutputStrength(_bustedRuleStartTick, BustedRuleDuration, Rule.Busted, s => BustedRuleOutputStrength = s);
        TryRecoverRuleOutputStrength(_wastedRuleStartTick, WastedRuleDuration, Rule.Wasted, s => WastedRuleOutputStrength = s);
        TryRecoverRuleOutputStrength(_miTangRuleStartTick, MiTangRuleDuration, Rule.MiTang, s => MiTangRuleOutputStrength = s);

        ChangeTimer();
    }

    private void ChangeTimer()
    {
        long min = long.MaxValue;
        foreach (var value in _dueTime.Values)
        {
            if (value > 0 && value < min)
            {
                min = value;
            }
        }
        if (min != long.MaxValue)
        {
            _timer.Change(min / TimeSpan.TicksPerMillisecond, Timeout.Infinite);
        }
    }

    private void TryRecoverRuleOutputStrength(long ruleStartTick, int seconds, Rule rule, Action<int> setOutputStrength)
    {
        if (ruleStartTick == 0)
            return;

        var dueTime = ruleStartTick - _stopwatch.ElapsedTicks + seconds * TimeSpan.TicksPerSecond;
        if (dueTime <= 0)
            setOutputStrength.Invoke(0);
        _dueTime[rule] = dueTime;
    }

    protected override int GetMaxStrength()
    {
        int[] strengths =
        [
            0, DamageRuleOutputStrength, BustedRuleOutputStrength, WastedRuleOutputStrength, MiTangRuleOutputStrength
        ];
        return strengths.Max();
    }

    protected override int SumStrength()
    {
        int result = 0;
        int[] strengths =
        [
            DamageRuleOutputStrength, BustedRuleOutputStrength, WastedRuleOutputStrength, MiTangRuleOutputStrength
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

    private void ComputeDamageRuleOutputStrength()
    {
        if (!IsEnable || !DamageRuleEnable)
        {
            DamageRuleOutputStrength = 0;
        }
        else if ((DamageRuleThreshold > 0 && _playerTookDamageTotal > DamageRuleThreshold)
            || (DamageRuleThreshold < 0 && _playerTookDamageTotal < DamageRuleThreshold))
        {
            int multiple = (int)(_playerTookDamageTotal / DamageRuleThreshold);
            _playerTookDamageTotal %= DamageRuleThreshold;
            int delta = multiple * DamageRuleStrength;
            DamageRuleOutputStrength = DamageRuleOutputStrength + delta;
        }
        else
        {
            return;
        }
    }

    private void Monitor_PlayerTookDamage(object? sender, PlayerTookDamageEventArgs e)
    {
        var damage = e.Damage;
        WeakReferenceMessenger.Default.Send(new Log($"汤米受到了{damage:F2}点伤害"));

        if (!DamageRuleEnable || DamageRuleDuration <= 0)
            return;

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
        ComputeDamageRuleOutputStrength();
    }

    private void Monitor_PlayerBusted(object? sender, PlayerBustedEventArgs e)
    {
        var level = e.WantedLevel;
        WeakReferenceMessenger.Default.Send(new Log($"汤米被抓了，痛失{level}枚好市民勋章"));

        if (!IsEnable || !BustedRuleEnable || BustedRuleDuration <= 0)
        {
            BustedRuleOutputStrength = 0;
        }
        else
        {
            BustedRuleOutputStrength = BustedRuleOutputStrength + level * BustedRuleStrength;
        }
    }

    private void Monitor_PlayerWasted(object? sender, PlayerWastedEventArgs e)
    {
        var isMiTang = e.IsMiTang;
        WeakReferenceMessenger.Default.Send(new Log(isMiTang ? "汤米变成了米汤" : "汤米浪费了"));

        if (isMiTang)
        {
            if (!IsEnable || !MiTangRuleEnable || MiTangRuleDuration <= 0)
            {
                MiTangRuleOutputStrength = 0;
            }
            else
            {
                MiTangRuleOutputStrength = MiTangRuleOutputStrength + MiTangRuleStrength;
            }
        }
        else
        {
            if (!IsEnable || !WastedRuleEnable || WastedRuleDuration <= 0)
            {
                WastedRuleOutputStrength = 0;
            }
            else
            {
                WastedRuleOutputStrength = WastedRuleOutputStrength + WastedRuleStrength;
            }
        }
    }

    private void Monitor_PlayerWantedLevelChanged(object? sender, PlayerWantedLevelChangedEventArgs e)
    {
        var diff = e.Diff;
        WeakReferenceMessenger.Default.Send(new Log(diff > 0 ? $"汤米获得了{diff}枚好市民勋章" : $"汤米丢失了{-diff}枚好市民勋章"));
    }

    private void Monitor_PlayerFellOffBike(object? sender, PlayerFellOffBikeEventArgs e)
    {
        var vehicleId = e.VehicleId;
        var vehicleType = e.VehicleType;
        var vehicleName = e.VehicleName;
        WeakReferenceMessenger.Default.Send(new Log($"汤米从{vehicleName}上摔下来了。{(short)vehicleId} 0x{(byte)vehicleType: X}"));
    }

    private enum Rule
    {
        Damage, Busted, Wasted, MiTang, WantedLevel, FellOffBike
    }
}
