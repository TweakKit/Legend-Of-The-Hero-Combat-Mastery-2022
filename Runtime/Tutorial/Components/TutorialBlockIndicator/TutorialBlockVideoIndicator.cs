using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace Runtime.Tutorial
{
    public class TutorialBlockVideoIndicator : TutorialBlockTimeIndicator<TutorialBlockVideoIndicatorData>
    {
        #region Members

        public VideoPlayer videoPlayer;
        public RawImage videoRawImage;
        public RenderTexture videoRenderTexture;
        private int _videoClipIndex = 0;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            videoPlayer.loopPointReached += OnVideoClipCompleted;
            if (OwnerBlockIndicatorData.videoClips.Count > 0)
            {
                videoPlayer.clip = OwnerBlockIndicatorData.videoClips[0];
                videoPlayer.Play();
            }
        }

        private void OnVideoClipCompleted(VideoPlayer videoPlayer)
        {
            this.videoPlayer.Stop();
            if (_videoClipIndex + 1 >= OwnerBlockIndicatorData.videoClips.Count)
            {
                FinishCutscene();
                return;
            }
            _videoClipIndex++;
            this.videoPlayer.clip = OwnerBlockIndicatorData.videoClips[_videoClipIndex];
            this.videoPlayer.Play();
        }

        private void FinishCutscene()
            => TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);

        #endregion Class Method
    }
}