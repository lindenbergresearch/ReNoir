#region

using Godot;
using Renoir;
using static Renoir.PropertyPool;

#endregion


public class PlayerInfoText : ParallaxBackground, IPropertyChangeHandler {

	[GNode("CoinsLabel")]
	private Label CoinsLabel;

	[GNode("LevelNameLabel")]
	private Label LevelNameLabel;

	[GNode("LivesLabel")]
	private Label LivesLabel;

	[GNode("ScoreLabel")]
	private Label ScoreLabel;

	[GNode("TimeLabel")]
	private Label TimeLabel;

	[GNode("FPSLabel")]
	private Label FPSLabel;


	public void OnPropertyChange<T>(Property<T> sender, PropertyEventArgs<T> args) {
		FPSLabel.Text = Node2D.FPS;
		ScoreLabel.Text = Mario2D.TotalScore;
		TimeLabel.Text = Level.Time;
		CoinsLabel.Text = Mario2D.CollectedCoins;
		LivesLabel.Text = Mario2D.LivesLeft;
		LevelNameLabel.Text = Level.Name;
	}


	public override void _Ready() {
		this.Init();

		AddSubscription("main.*", this);
	}

}
