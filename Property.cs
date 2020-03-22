using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
///     Maintains a registered properties
/// </summary>
public class PropertyPool {
	private static readonly Dictionary<string, object> pool = new Dictionary<string, object>();


	/// <summary>
	///     Registers a new property at the property-pool.
	/// </summary>
	/// <param name="property">The property to update</param>
	public static void Register<T>(Property<T> property) {
		if (pool.ContainsKey(property.Name)) pool[property.Name] = property;
		else pool.Add(property.Name, property);
	}


	/// <summary>
	///     Get a property by it's name ID
	/// </summary>
	/// <param name="name">The properties name ID</param>
	/// <returns></returns>
	public static Property<T> Pull<T>(string id) {
		var tmp = pool[id];

		if (tmp is Property<T> t) return t;

		return (Property<T>) tmp;
	}


	/// <summary>
	///     Removes a property from the pool.
	/// </summary>
	/// <param name="property">The property to update</param>
	public static void Unregister<T>(Property<T> property) {
		pool.Remove(property.Name);
	}


	/// <summary>
	///     Returns the pool's content as string representation.
	/// </summary>
	/// <returns></returns>
	public static string AsString() {
		var s = "[";

		foreach (var property in pool) s += property + (property.Key != pool.Last().Key ? ", " : "");

		return s;
	}


	/// <summary>
	///     Factory method for creating a property and automatically registering it..
	/// </summary>
	/// <param name="name"></param>
	/// <param name="value"></param>
	/// <param name="group"></param>
	/// <param name="locked"></param>
	/// <returns></returns>
	public static Property<T> RegisterNewProperty<T>(string name, T value, string group = "", bool locked = false) {
		var p = new Property<T>(name, value, group, locked);
		Register(p);
		return p;
	}


	public static Property<dynamic> Cast(object obj, Type castTo) {
		return Convert.ChangeType(obj, castTo) as Property<dynamic>;
	}


	/// <summary>
	///     Add a subscription to one or more properties defined by a matching string.
	/// </summary>
	/// <param name="subscriber"></param>
	/// <param name="match"></param>
	public static void AddSubscription(IPropertyChangeListener subscriber, string match) {
		var isgroup = match.Trim().StartsWith("$");
		var r = @"^" + match.Trim().Replace(".", @"\.").Replace("$", @"\$").Replace("*", @".*") + "$";
		var Pattern = new Regex(r, RegexOptions.Compiled);

		foreach (var pair in pool) {
			var id = pair.Key;
			var property = pair.Value;

			MatchCollection matches;

			if (isgroup) {
				var group = Util.GetPropertyByName("Group", property);
				matches = Regex.Matches(group, r, RegexOptions.Singleline);
			}
			else {
				matches = Regex.Matches(id, r, RegexOptions.Singleline);
			}

			if (matches.Count > 0)
				Util.InvokeMethodByName("Subscribe", new[] {subscriber}, property);
		}
	}
}


/// <summary>
///     Interface for subscriber classes.
/// </summary>
public interface IPropertyChangeListener {
	/// <summary>
	///     Called upon an event has been fired.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="args"></param>
	void OnPropertyChange<T>(Property<T> sender, PropertyEventArgs<T> args);
}

/// <summary>
///     Property change event data.
/// </summary>
public class PropertyEventArgs<T> : EventArgs {
	public PropertyEventArgs(T old, T @new) {
		Old = old;
		New = @new;
	}


	public T Old { get; }
	public T New { get; }


	/// <summary>
	///     Pretty print this event.
	/// </summary>
	/// <returns></returns>
	public override string ToString() {
		var o = Old != null ? Old.ToString() : "<empty>";
		var n = New != null ? New.ToString() : "<empty>";
		return $"({o} => {n})";
	}
}


/// <summary>
///     Encapsulates a property, binds it to a name and provide an event to catch manipulating.
/// </summary>
public class Property<T> {
	/// <summary>
	///     Event handler delegate.
	/// </summary>
	/// <param name="sender">The property where the event is raised.</param>
	/// <param name="args">Change data.</param>
	public delegate void ChangeEventHandler(Property<T> sender, PropertyEventArgs<T> args);

	private static long lastid = 100;

	private T _value;


	/// <summary>
	///     Constructs a property.
	/// </summary>
	/// <param name="name">The name of the property</param>
	/// <param name="value">The properties value</param>
	/// <param name="group">The properties group membership</param>
	/// <param name="locked">Write-lock (default is false)</param>
	public Property(string name, T value, string group = "", bool locked = false) {
		Name = name;
		Group = group;
		_value = value;
		Locked = locked;
		ID = lastid++;
		Console.Out.WriteLine(name + " => " + lastid + " - " + ID);
	}


	/// <summary>
	///     Constructs a property with a default value.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="locked"></param>
	public Property(string name, string group = "", bool locked = false) {
		Name = name;
		Group = group;
		Locked = locked;
		ID = lastid++;
		Console.Out.WriteLine(name + " => " + lastid + " - " + ID);
	}


	public T Value {
		get => _value;
		set {
			var newVal = value;

			ExecuteTrigger(newVal);
			ExecuteTransformTrigger(newVal);

			// raise event on changing values!
			OnPropertyChange(new PropertyEventArgs<T>(_value, newVal));

			_value = newVal;
		}
	}

	public long Modcount { get; set; }

	public string Name { get; }
	public string Group { get; }
	public string Format { get; set; } = "";
	public long ID { get; }
	public bool Locked { get; set; }

	/// <summary>
	///     Dynamic transformation trigger list. Contains all transformation trigger.
	/// </summary>
	public List<(Func<T, bool>, Func<T, T>)> TransformTriggers { get; } = new List<(Func<T, bool>, Func<T, T>)>();

	/// <summary>
	///     Dynamic trigger list. Contains all constraint trigger.
	/// </summary>
	public List<(Func<T, bool>, Action<T>)> Triggers { get; } = new List<(Func<T, bool>, Action<T>)>();

	/// <summary>
	///     The change event bound to the event handler delegate.
	/// </summary>
	public event ChangeEventHandler RaiseChangeEvent;


	/// <summary>
	///     Subscribe to event.
	/// </summary>
	/// <param name="subscriber"></param>
	public void Subscribe(IPropertyChangeListener handler) {
		RaiseChangeEvent += handler.OnPropertyChange;
	}


	/// <summary>
	///     UnSubscribe to event.
	/// </summary>
	/// <param name="handler"></param>
	public void Unsubscribe(IPropertyChangeListener handler) {
		RaiseChangeEvent -= handler.OnPropertyChange;
	}


	/// <summary>
	///     Return the properties value as formatted string using
	///     the internal setup Format property.
	/// </summary>
	/// <returns></returns>
	public string Formatted() {
		return string.Format(Format, _value);
	}


	/// <summary>
	///     Return the properties value as formatted string using
	///     the given format specifier.
	/// </summary>
	/// <param name="format"></param>
	/// <returns></returns>
	public string Formatted(string format) {
		return string.Format(format, _value);
	}


	/// <summary>
	///     Event handler. Propagate event to all subscriber.
	/// </summary>
	/// <param name="e"></param>
	protected virtual void OnPropertyChange(PropertyEventArgs<T> e) {
		var handler = RaiseChangeEvent;

		Modcount++;

		// raise event
		if (handler != null) handler(this, e);
	}


	/// <summary>
	///     String representation of the property.
	/// </summary>
	/// <returns></returns>
	public override string ToString() {
		var typeStr = typeof(T).ToString().Split('.');
		var valStr = Value != null ? Value.ToString() : "null";
		var groupStr = Group.Length > 1 ? $"group: {Group}({Name})" : $"{Name}";

		if (Format.Length > 0 && Value != null) valStr = Formatted();

		return $"[Name='{Name}' ID={ID} value={typeStr[typeStr.Length - 1]}({valStr})" + (Locked ? " locked=true" : "") +
			   $" handler={RaiseChangeEvent?.GetInvocationList()?.Length}" +
			   $" trigger={Triggers.Count} transforms={TransformTriggers.Count}" + "]";
	}


	/// <summary>
	///     Adds a trigger handler which is raised if 'cond' becomes true and transforms
	///     it's value via 'handler'.
	/// </summary>
	/// <param name="cond">The condition closure.</param>
	/// <param name="handler">The handler closure. Should return null on no transformation.</param>
	public void AddTransformTrigger(Func<T, bool> cond, Func<T, T> handler) {
		TransformTriggers.Add((cond, handler));
	}


	/// <summary>
	///     Checks all transform-triggers for match and applies it's
	///     transformation on success.
	/// </summary>
	/// <param name="t">The value to check against all riggers.</param>
	private void ExecuteTransformTrigger(T t) {
		foreach (var (cond, handler) in TransformTriggers)
			if (cond(t)) {
				var tmp = handler(t);
				if (tmp != null) t = tmp;
			}
	}


	/// <summary>
	///     Adds a trigger handler which is raised if 'cond' becomes true.
	///     Does not transforms anything.
	/// </summary>
	/// <param name="cond">The condition closure.</param>
	/// <param name="handler">The handler closure. Should return null on no transformation.</param>
	public void AddTrigger(Func<T, bool> cond, Action<T> handler) {
		Triggers.Add((cond, handler));
	}


	/// <summary>
	///     Checks all triggers for match and call it's handler on success.
	/// </summary>
	/// <param name="t">The value to check against all riggers.</param>
	private void ExecuteTrigger(T t) {
		foreach (var (cond, handler) in Triggers)
			if (cond(t))
				handler(t);
	}


	/*===== IMPLICIT CONVERSATIONS =================================================================================*/
	public static implicit operator int(Property<T> p) {
		if (p.Value is int i) return i;

		return 0;
	}


	public static implicit operator long(Property<T> p) {
		if (p.Value is long l) return l;

		return 0;
	}


	public static implicit operator float(Property<T> p) {
		if (p.Value is float f) return f;

		return 0;
	}


	public static implicit operator double(Property<T> p) {
		if (p.Value is double d) return d;

		return 0;
	}


	public static implicit operator string(Property<T> p) {
		return p.Value.ToString();
	}
	/*===== IMPLICIT CONVERSATIONS =================================================================================*/


	/*===== OPERATOR CONVERSATIONS =================================================================================*/
	public static Property<T> operator +(Property<T> p, dynamic n) {
		p.Value = p._value + n;
		return p;
	}


	public static Property<T> operator -(Property<T> p, dynamic n) {
		p.Value = p._value - n;
		return p;
	}


	public static Property<T> operator *(Property<T> p, dynamic n) {
		p.Value = p._value * n;
		return p;
	}


	public static Property<T> operator /(Property<T> p, dynamic n) {
		p.Value = p._value / n;
		return p;
	}


	public static Property<T> operator ==(Property<T> p, dynamic n) {
		return p._value == n;
	}


	public static Property<T> operator !=(Property<T> p, dynamic n) {
		return p._value != n;
	}


	public static Property<T> operator >(Property<T> p, dynamic n) {
		p.Value = p._value > n;
		return p;
	}


	public static Property<T> operator <(Property<T> p, dynamic n) {
		p.Value = p._value < n;
		return p;
	}


	public static Property<T> operator >=(Property<T> p, dynamic n) {
		p.Value = p._value >= n;
		return p;
	}


	public static Property<T> operator <=(Property<T> p, dynamic n) {
		p.Value = p._value <= n;
		return p;
	}


	public static Property<T> operator ++(Property<T> p) {
		return p + 1;
	}


	public static Property<T> operator --(Property<T> p) {
		return p - 1;
	}


	public static Property<T> operator -(Property<T> p) {
		if (p is Property<int> i) {
			i._value = -i._value;
			return p;
		}

		if (p is Property<long> l) {
			l._value = -l._value;
			return p;
		}

		if (p is Property<float> f) {
			f._value = -f._value;
			return p;
		}

		if (p is Property<double> d) {
			d._value = -d._value;
			return p;
		}

		throw new InvalidCastException(
			$"Type: {p.GetType()} can't be casted to int|long|float|double to apply unary (-) operator.");

		return p;
	}


	public static Property<T> operator !(Property<T> p) {
		if (p is Property<bool> pbool)
			pbool.Value = !pbool.Value;
		else
			throw new InvalidCastException($"Type: {p.GetType()} can't be casted to bool to apply unary (!) operator.");

		return p;
	}


	/*===== OPERATOR CONVERSATIONS =================================================================================*/
}
