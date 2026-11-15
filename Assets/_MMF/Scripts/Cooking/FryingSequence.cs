using System;
namespace MMF.Cooking
{
    /// <summary>Pure two-round frying rules, independent of input and rendering.</summary>
    public sealed class FryingSequence
    {
        private readonly Random _random;
        private readonly float _seconds;
        private readonly char[] _letters;
        public int Length => _letters.Length;
        public int Progress { get; private set; }
        public int Round { get; private set; }
        public float Remaining { get; private set; }
        public bool Active { get; private set; }
        public bool Flipping { get; private set; }
        public bool Cooked { get; private set; }
        public bool Burnt { get; private set; }
        /// <summary>Creates configurable rounds with an injectable random source.</summary>
        public FryingSequence(int length, float seconds, Random random)
        {
            if (length < 1 || seconds <= 0f) throw new ArgumentOutOfRangeException();
            _letters = new char[length]; _seconds = seconds;
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }
        /// <summary>Returns the displayed letter at an index.</summary>
        public char LetterAt(int index) => _letters[index];
        /// <summary>Starts fresh with a full first-round timer.</summary>
        public void Start()
        {
            Round = 1; Active = true; Flipping = Cooked = Burnt = false; NewRound();
        }
        private void NewRound()
        {
            Progress = 0; Remaining = _seconds;
            for (int i = 0; i < Length; i++) _letters[i] = "WASD"[_random.Next(4)];
        }
        /// <summary>Wrong letters do not change progress.</summary>
        public void Press(char letter)
        {
            if (!Active || Flipping || letter != _letters[Progress]) return;
            if (++Progress < Length) return;
            if (Round == 1) Flipping = true;
            else { Cooked = true; Active = false; }
        }
        /// <summary>Begins the second round after the visual flip.</summary>
        public void FinishFlip()
        {
            if (!Active || !Flipping) return;
            Flipping = false; Round = 2; NewRound();
        }
        /// <summary>Animation does not consume either round's input time.</summary>
        public void Tick(float seconds)
        {
            if (!Active || Flipping) return;
            Remaining = Math.Max(0f, Remaining - Math.Max(0f, seconds));
            if (Remaining == 0f) { Burnt = true; Active = false; }
        }
        /// <summary>Clears progress and stops the timer.</summary>
        public void Cancel()
        {
            Active = Flipping = Cooked = Burnt = false;
            Round = Progress = 0; Remaining = 0f; Array.Clear(_letters, 0, Length);
        }
    }
}
