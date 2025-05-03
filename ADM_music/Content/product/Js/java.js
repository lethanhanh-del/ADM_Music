const data = [
	{
		img: "https://i.scdn.co/image/ab67616d00001e028898ca25107af040f275a749",
		name: "Bông Hoa Đẹp Nhất",
		author: "Quân A.P",
		code: "KCdZZUWJWMPrduiL",
	},
	{
		img: "https://i.scdn.co/image/ab67616d00001e02625fb735c4fc0aa2b69a6639",
		name: "Dễ Đến Dễ Đi",
		author: "Quang Hùng Masterded",
		code: "GLVyHNdeX031pJLn",
	},
	{
		img: "https://i.scdn.co/image/ab67616d00001e02bb0e3a4f2972a198cf701860",
		name: "Vội Vàng",
		author: "Tạ Quang Thắng",
		code: "LEwWnmj9UpeZY5js",
	},
	{
		img: "https://i.scdn.co/image/ab67616d00001e027e209cd9baf90c200f7d2043",
		name: "Lỡ Say Bye Là Bye",
		author: "Lemese, Changg",
		code: "NnLI8WZXoxOaBIq0",
	},
	{
		img: "https://i.scdn.co/image/ab67616d00001e027fa39203a5f3e8abff2e2368",
		name: "Giấc mơ khác",
		author: "Chillies",
		code: "FIkZoJGjpsqoJ7ey",
	},
	{
		img: "https://i.scdn.co/image/ab67616d00001e02bebe2d4754ba6abfc0051f1d",
		name: "Phải chăng em đã yêu",
		author: "Juky San",
		code: "hHx5Sn6HZC9XjRGo",
	},
];

let selectedIndex = Math.floor(Math.random() * data.length);

document.addEventListener("DOMContentLoaded", function () {
	const audio = document.getElementById("main-audio");

	const imgArt = document.getElementById("img-art");
	const musicName = document.getElementById("music-name");
	const authorName = document.getElementById("author-name");

	const nextMusicBtn = document.getElementById("next-btn");
	const prevMusicBtn = document.getElementById("prev-btn");
	const playPauseBtn = document.getElementById("play-pause-btn");
	const randomBtn = document.getElementById("random-btn");
	const fullscreenBtn = document.getElementById("fullscreen-btn");
	const repeatBtn = document.getElementById("repeat-btn");
	const volumeBtn = document.getElementById("volume-btn");

	const volumeBar = document.getElementById("volume-bar");
	const volumeFilled = document.getElementById("volume-filled");

	const progressBar = document.getElementById("progress-bar");
	const progressPlayed = document.getElementById("progress-played");

	const currentTimeText = document.getElementById("current-time");
	const maxDurationText = document.getElementById("max-duration");

	const musicCards = document.querySelectorAll(".music");

	let isDragging = false;
	let isRepeat = false;
	let isMute = false;
	let volume = 1;

	musicCards.forEach((music) =>
		music.addEventListener("click", function (e) {
			let tmp = data.findIndex(x => x.code == e.target.id);
            if (tmp >= 0) {
                selectedIndex = tmp;
                setMusic(selectedIndex);
            }
		})
	);

	selectedIndex = Math.floor(Math.random() * data.length);
	setMusic(selectedIndex);

	fullscreenBtn.addEventListener("click", function () {
		if (screen.width === window.innerWidth) {
			document.exitFullscreen();
		} else {
			document.body.requestFullscreen();
		}
	});

	volumeBtn.addEventListener("click", function () {
		if (isMute) {
			isMute = false;
			if (volume == 0) {
				volume = 0.5;
			}
			audio.volume = volume;
		} else {
			isMute = true;
			volume = audio.volume;
			audio.volume = 0;
		}
	});

	audio.addEventListener("volumechange", function () {
		volumeFilled.style.width = `${audio.volume * 100}%`;
		if (audio.volume == 0) {
			isMute = true;
			volumeBtn.innerHTML = `<i class="fa-solid fa-volume-xmark"></i>`;
		} else {
			isMute = false;
			volumeBtn.innerHTML = `<i class="fa-solid fa-volume-high"></i>`;
		}
	});

	document.addEventListener("fullscreenchange", function (e) {
		if (document.fullscreenElement) {
			fullscreenBtn.innerHTML = '<i class="fa-solid fa-compress"></i>';
		} else {
			fullscreenBtn.innerHTML = '<i class="fa-solid fa-expand"></i>';
		}
	});

	playPauseBtn.addEventListener("click", function () {
		if (audio.paused) {
			audio.play();
		} else {
			audio.pause();
		}
	});

	repeatBtn.addEventListener("click", function () {
		if (isRepeat) {
			isRepeat = false;
			audio.loop = false;
			repeatBtn.innerHTML = `<i class="fa fa-repeat"></i>`;
		} else {
			isRepeat = true;
			audio.loop = true;
			repeatBtn.innerHTML = `<i class="fa fa-repeat-1"></i>`;
		}
	});

	nextMusicBtn.addEventListener("click", function () {
		let tmp = selectedIndex + 1;
		if (!isNullOrUndefined(data[tmp])) {
			selectedIndex = tmp;
			setMusic(selectedIndex);
			audio.play();
		}
	});

	prevMusicBtn.addEventListener("click", function () {
		let tmp = selectedIndex - 1;
		if (!isNullOrUndefined(data[tmp])) {
			selectedIndex = tmp;
			setMusic(selectedIndex);
			audio.play();
		}
	});

	randomBtn.addEventListener("click", function () {
		selectedIndex = Math.floor(Math.random() * data.length);
		setMusic(selectedIndex);
		audio.play();
	});

	audio.addEventListener("loadedmetadata", function () {
		maxDurationText.innerText = formatTime(audio.duration);
	});

	audio.addEventListener("timeupdate", function () {
		if (!isDragging) {
			const currentTime = audio.currentTime;
			const duration = audio.duration;
			const progressPercent = Math.min(currentTime / duration, 1);
			progressPlayed.style.width = `${progressPercent * 100}%`;
			currentTimeText.innerText = formatTime(currentTime);
			if (duration == currentTime) {
				if (!isRepeat) {
					playPauseBtn.innerHTML = '<i class="fa fa-play-circle"></i>';
				} else {
					audio.play();
				}
			} else {
				if (audio.paused) {
					playPauseBtn.innerHTML = '<i class="fa fa-play-circle"></i>';
				} else {
					playPauseBtn.innerHTML = '<i class="fa fa-pause-circle"></i>';
				}
			}
		}
	});

	progressBar.addEventListener("click", function (e) {
		const progressWidth = progressBar.clientWidth;
		const offsetX = e.clientX - progressBar.getBoundingClientRect().left;
		const progressPercent = Math.min(offsetX / progressWidth, 1);
		progressPlayed.style.width = `${progressPercent * 100}%`;
		currentTimeText.innerText = formatTime(progressPercent * audio.duration);
		audio.currentTime = progressPercent * audio.duration;
	});

	progressBar.addEventListener("mousedown", function (e) {
		e.preventDefault();
		isDragging = true;
		document.addEventListener("mousemove", updateProgress);
		document.addEventListener("mouseup", stopDragging);
	});

	volumeBar.addEventListener("click", function (e) {
		const width = volumeBar.clientWidth;
		const offsetX = e.clientX - volumeBar.getBoundingClientRect().left;
		let percent = Math.min(offsetX / width, 1);
		if (percent < 0) {
			percent = 0;
		}
		audio.volume = percent;
	});

	volumeBar.addEventListener("mousedown", function (e) {
		e.preventDefault();
		isDragging = true;
		document.addEventListener("mousemove", changeVolume);
		document.addEventListener("mouseup", stopChangeVolume);
	});

	function setMusic(selectedIndex) {
		const music = data[selectedIndex];
		audio.src = `Audio\\${music.code}.mp3`;
		imgArt.src = music.img;
		musicName.innerText = music.name;
		authorName.innerText = music.author;
		audio.currentTime = 0;
		progressPlayed.style.width = "0%";
	}

	function changeVolume(e) {
		if (isDragging) {
			const width = volumeBar.clientWidth;
			const offsetX = e.clientX - volumeBar.getBoundingClientRect().left;
			let percent = Math.min(offsetX / width, 1);
			if (percent < 0) {
				percent = 0;
			}
			audio.volume = percent;
		}
	}

	function stopChangeVolume(e) {
		if (isDragging) {
			document.removeEventListener("mousemove", updateProgress);
			document.removeEventListener("mouseup", stopDragging);
			isDragging = false;
		}
	}

	function updateProgress(e) {
		if (isDragging) {
			const progressWidth = progressBar.clientWidth;
			const offsetX = e.clientX - progressBar.getBoundingClientRect().left;
			let progressPercent = Math.min(offsetX / progressWidth, 1);
			if (progressPercent < 0) {
				progressPercent = 0;
			}
			progressPlayed.style.width = `${progressPercent * 100}%`;
			currentTimeText.innerText = formatTime(progressPercent * audio.duration);
		}
	}

	function stopDragging(e) {
		if (isDragging) {
			const progressWidth = progressBar.clientWidth;
			const offsetX = e.clientX - progressBar.getBoundingClientRect().left;
			let progressPercent = Math.min(offsetX / progressWidth, 1);
			if (progressPercent < 0) {
				progressPercent = 0;
			}
			audio.currentTime = progressPercent * audio.duration;
			document.removeEventListener("mousemove", updateProgress);
			document.removeEventListener("mouseup", stopDragging);
			isDragging = false;
		}
	}
});

function formatTime(time) {
	const minutes = Math.floor(time / 60);
	const seconds = Math.floor(time % 60);
	return `${minutes}:${seconds < 10 ? "0" : ""}${seconds}`;
}

function isNullOrUndefined(value) {
	return value === undefined || value === null;
}
