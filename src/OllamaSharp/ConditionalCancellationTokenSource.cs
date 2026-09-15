/*
using System;
using System.Collections.Generic;
using System.Text;

namespace OllamaSharp;

public class ConditionalCancellationTokenSource : CancellationTokenSource
{
	private readonly Timer _timer;
	private TimeSpan _delay;


	public ConditionalCancellationTokenSource(int millisecondsDelay) : this(TimeSpan.FromMilliseconds(millisecondsDelay))
	{
	}

	public ConditionalCancellationTokenSource(TimeSpan delay) : base()
	{
		this._delay = delay;
		_timer = new Timer(this.TimerCallback, new object(), (int)this._delay.TotalMilliseconds, 0);
	}

	private void TimerCallback(object state)
	{
		if (this.Token.IsCancellationRequested)
		{
			return;
		}

		if (this.OnTimeoutElapsed != null)
		{
			var aaaa1 = new ConditionalCancellationTokenSourceEventArgs { ElapsedTimeout = _delay, NewTimeout = TimeSpan.Zero };
			this.OnTimeoutElapsed(this, aaaa1);
			if (aaaa1.NewTimeout > TimeSpan.Zero)
			{
				if (aaaa1.NewTimeout != _delay)
				{
					_delay = aaaa1.NewTimeout;
					_timer.Change((int)this._delay.TotalMilliseconds, 0);
				}

				return;
			}
		}

		this.Cancel();
	}

	public EventHandler<ConditionalCancellationTokenSourceEventArgs> OnTimeoutElapsed { get; set; }

}

public class ConditionalCancellationTokenSourceEventArgs : EventArgs
{
	public TimeSpan ElapsedTimeout { get; set; }

	public TimeSpan NewTimeout { get; set; }
}

*/