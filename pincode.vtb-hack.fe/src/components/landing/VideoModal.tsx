"use client";

import { Dialog, DialogContent } from "@pin-code/ui-kit";
import { X } from "lucide-react";

type Props = {
	isOpen: boolean;
	onClose: () => void;
};

export const VideoModal = ({ isOpen, onClose }: Props) => {
	return (
		<Dialog open={isOpen} onOpenChange={onClose}>
			<DialogContent className="max-w-6xl p-0 bg-black border-0 overflow-hidden">
				<div className="relative aspect-video">
					<video className="h-full w-full" controls autoPlay preload="metadata" poster="/images/demo.png">
						<source src="https://storage.yandexcloud.net/hackathon-1/hack.mp4" type="video/mp4" />
						<p className="text-white p-4">
							Ваш браузер не поддерживает воспроизведение видео.{" "}
							<a
								href="https://storage.yandexcloud.net/hackathon-1/hack.mp4"
								className="text-blue-400 underline"
							>
								Скачайте видео
							</a>
							.
						</p>
					</video>
				</div>
			</DialogContent>
		</Dialog>
	);
};
