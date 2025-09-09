"use client";

import * as React from "react";
import Image from "next/image";
import { CheckIcon, ExternalLinkIcon, X } from "lucide-react";
import { AnimatePresence, motion, type PanInfo } from "framer-motion";
import { Slot } from "@radix-ui/react-slot";

import { AspectRatio, Button, Checkbox, cn, Progress } from "@pin-code/ui-kit";

/**
 * Хук для отслеживания медиа-запросов
 */
function useMediaQuery(query: string) {
	const [matches, setMatches] = React.useState<boolean | null>(null);

	React.useEffect(() => {
		const media = window.matchMedia(query);
		setMatches(media.matches);

		const listener = (e: MediaQueryListEvent) => setMatches(e.matches);
		media.addEventListener("change", listener);
		return () => media.removeEventListener("change", listener);
	}, [query]);

	return matches ?? false;
}

/**
 * Хук для управления видимостью функции через localStorage
 */
function useFeatureVisibility(featureId: string) {
	const [isVisible, setIsVisible] = React.useState<boolean | null>(null);

	React.useEffect(() => {
		const storedValue = localStorage.getItem(`feature_${featureId}`);
		setIsVisible(storedValue ? JSON.parse(storedValue) : true);
	}, [featureId]);

	const hideFeature = () => {
		localStorage.setItem(`feature_${featureId}`, JSON.stringify(false));
		setIsVisible(false);
	};

	return { isVisible: isVisible === null ? false : isVisible, hideFeature };
}

/**
 * Хук для обработки свайпов
 */
function useSwipe(onSwipe: (direction: "left" | "right") => void) {
	const handleDragEnd = (_event: MouseEvent | TouchEvent | PointerEvent, info: PanInfo) => {
		if (info.offset.x > 100) {
			onSwipe("right");
		} else if (info.offset.x < -100) {
			onSwipe("left");
		}
	};

	return { handleDragEnd };
}

/**
 * Анимации для слайдов
 */

const slideInOut = (direction: 1 | -1) => ({
	initial: { opacity: 0, x: 20 * direction },
	animate: { opacity: 1, x: 0 },
	exit: { opacity: 0, x: -20 * direction },
	transition: { duration: 0.3, ease: [0.25, 0.1, 0.25, 1] as const },
});

/**
 * Компонент фоновых анимаций в стиле warp
 */
function WarpAnimations() {
	const enterDuration = 0.5;
	const exitDuration = 0.25;

	return (
		<>
			<motion.div
				className="absolute top-[100%] left-[25%] h-1/2 w-1/2 origin-center rounded-full blur-lg will-change-transform"
				initial={{
					scale: 0,
					opacity: 1,
					backgroundColor: "hsl(44,61%,59%)",
				}}
				animate={{
					scale: 10,
					opacity: 0.2,
					backgroundColor: "hsl(44,95%,61%)",
					transition: {
						duration: enterDuration,
						opacity: { duration: enterDuration, ease: "easeInOut" },
					},
				}}
				exit={{
					scale: 0,
					opacity: 1,
					backgroundColor: "hsl(45, 80%, 60%)",
					transition: { duration: exitDuration },
				}}
			/>
			<motion.div
				className="absolute top-[-25%] left-[-50%] h-full w-full rounded-full bg-gradient-to-br from-black via-amber-400/80 to-amber-500/90 blur-[100px]"
				initial={{ opacity: 0 }}
				animate={{
					opacity: 0.9,
					scale: [1, 0.7, 1],
					transition: {
						duration: enterDuration,
						scale: {
							duration: 15,
							repeat: Infinity,
							repeatType: "loop",
							ease: "easeInOut",
							delay: 0.35,
						},
					},
				}}
				exit={{
					opacity: 0,
					transition: { duration: exitDuration },
				}}
			/>
			<motion.div
				className="absolute top-[25%] left-[50%] h-full w-full rounded-full bg-gradient-to-tl from-black via-yellow-600/80 to-amber-400/90 blur-[100px]"
				initial={{ opacity: 0 }}
				animate={{
					opacity: 0.9,
					scale: [1, 0.7, 1],
					transition: {
						duration: enterDuration,
						scale: {
							duration: 15,
							repeat: Infinity,
							repeatType: "loop",
							ease: "easeInOut",
							delay: 0.35,
						},
					},
				}}
				exit={{
					opacity: 0,
					transition: { duration: exitDuration },
				}}
			/>
		</>
	);
}

/**
 * Интерфейс для шага онбординга
 */
interface Step {
	title: string;
	short_description: string;
	full_description: string;
	action?: {
		label: string;
		onClick?: () => void;
		href?: string;
	};
	media?: {
		type: "image" | "video";
		src: string;
		alt?: string;
	};
}

/**
 * Props для основного компонента
 */
export interface WarpOnboardingProps {
	/** Массив шагов для онбординга */
	steps: Step[];
	/** Уникальный ID для сохранения состояния */
	featureId: string;
	/** Открыт ли онбординг */
	open: boolean;
	/** Обработчик изменения состояния открытия */
	onOpenChange: (open: boolean) => void;
	/** Обработчик завершения онбординга */
	onComplete?: () => void;
	/** Обработчик пропуска онбординга */
	onSkip?: () => void;
	/** Показывать ли прогресс-бар */
	showProgressBar?: boolean;
	/** Принудительный вариант отображения */
	forceVariant?: "mobile" | "desktop";
}

/**
 * Props для триггера
 */
export interface WarpOnboardingTriggerProps {
	asChild?: boolean;
	children: React.ReactNode;
}

/**
 * Контекст для онбординга
 */
type WarpOnboardingContextType = {
	open: boolean;
	setOpen: (open: boolean) => void;
	steps: Step[];
	featureId: string;
	onComplete?: () => void;
	onSkip?: () => void;
	showProgressBar?: boolean;
	forceVariant?: "mobile" | "desktop";
};

const WarpOnboardingContext = React.createContext<WarpOnboardingContextType | null>(null);

export function useWarpOnboardingContext() {
	const ctx = React.useContext(WarpOnboardingContext);
	if (!ctx) {
		throw new Error("WarpOnboarding components must be used inside <WarpOnboarding>");
	}
	return ctx;
}

/**
 * Основной компонент онбординга
 */
export function WarpOnboarding({
	steps,
	featureId,
	open,
	onOpenChange,
	onComplete,
	onSkip,
	showProgressBar = true,
	forceVariant,
	children,
}: WarpOnboardingProps & { children: React.ReactNode }) {
	const contextValue = React.useMemo<WarpOnboardingContextType>(
		() => ({
			open,
			setOpen: onOpenChange,
			steps,
			featureId,
			onComplete,
			onSkip,
			showProgressBar,
			forceVariant,
		}),
		[open, onOpenChange, steps, featureId, onComplete, onSkip, showProgressBar, forceVariant],
	);

	return <WarpOnboardingContext.Provider value={contextValue}>{children}</WarpOnboardingContext.Provider>;
}

/**
 * Триггер для запуска онбординга
 */
export function WarpOnboardingTrigger({ asChild = false, children }: WarpOnboardingTriggerProps) {
	const { setOpen } = useWarpOnboardingContext();
	const Comp = asChild ? Slot : "button";

	return (
		<Comp onClick={() => setOpen(true)} className={asChild ? undefined : "cursor-pointer"}>
			{children}
		</Comp>
	);
}

/**
 * Компонент содержимого онбординга
 */
export function WarpOnboardingContent() {
	const { open, setOpen, steps, featureId, onComplete, onSkip, showProgressBar, forceVariant } =
		useWarpOnboardingContext();

	const [currentStep, setCurrentStep] = React.useState(0);
	const [completedSteps, setCompletedSteps] = React.useState<number[]>([0]);
	const [direction, setDirection] = React.useState<1 | -1>(1);
	const [skipNextTime, setSkipNextTime] = React.useState(false);

	const isDesktopQuery = useMediaQuery("(min-width: 768px)");
	const isDesktop = forceVariant ? forceVariant === "desktop" : isDesktopQuery;
	const { isVisible, hideFeature } = useFeatureVisibility(featureId);

	// Закрываем диалог если функция скрыта
	React.useEffect(() => {
		if (!isVisible) {
			setOpen(false);
		}
	}, [isVisible, setOpen]);

	// Сброс состояния при открытии
	React.useEffect(() => {
		if (open) {
			setCurrentStep(0);
			setCompletedSteps([0]);
			setDirection(1);
			setSkipNextTime(false);
		}
	}, [open]);

	const handleNext = () => {
		setDirection(1);
		setCompletedSteps((prev) => (prev.includes(currentStep) ? prev : [...prev, currentStep]));
		if (currentStep < steps.length - 1) {
			setCurrentStep(currentStep + 1);
		} else {
			if (skipNextTime) {
				hideFeature();
			}
			setOpen(false);
			onComplete?.();
		}
	};

	const handlePrevious = () => {
		setDirection(-1);
		if (currentStep > 0) {
			setCurrentStep(currentStep - 1);
		}
	};

	const handleSkip = () => {
		if (skipNextTime) {
			hideFeature();
		}
		setOpen(false);
		onSkip?.();
	};

	const handleStepSelect = (index: number) => {
		setDirection(index > currentStep ? 1 : -1);
		setCompletedSteps((prev) => {
			const newCompletedSteps = new Set(prev);
			if (index > currentStep) {
				for (let i = currentStep; i <= index; i++) {
					newCompletedSteps.add(i);
				}
			}
			return Array.from(newCompletedSteps);
		});
		setCurrentStep(index);
	};

	const handleSwipe = (swipeDirection: "left" | "right") => {
		if (swipeDirection === "left") {
			handleNext();
		} else {
			handlePrevious();
		}
	};

	const { handleDragEnd } = useSwipe(handleSwipe);

	const renderActionButton = (action: Step["action"]) => {
		if (!action) return null;

		if (action.href) {
			return (
				<Button asChild className="w-full" size="sm" variant="outline">
					<a href={action.href} target="_blank" rel="noopener noreferrer">
						<span className="flex items-center gap-2">
							{action.label}
							<ExternalLinkIcon className="w-4 h-4" />
						</span>
					</a>
				</Button>
			);
		}

		return (
			<Button className="w-full" size="sm" variant="outline" onClick={action.onClick}>
				{action.label}
			</Button>
		);
	};

	// Не показываем, если функция скрыта
	if (!isVisible) {
		return null;
	}

	if (isDesktop) {
		// Desktop версия с warp анимациями
		return (
			<AnimatePresence>
				{open && (
					<motion.div
						className={cn("absolute z-[9998]")}
						initial={{ opacity: 0 }}
						animate={{ opacity: 1 }}
						exit={{ opacity: 0 }}
						transition={{ duration: 0.35, ease: [0.59, 0, 0.35, 1] }}
					>
						{/* Warp фон */}
						<div className="fixed inset-0 z-[9999] bg-black/20">
							<WarpAnimations />
						</div>

						<motion.div
							className="fixed inset-0 z-[10000] flex items-center justify-center"
							initial={{ opacity: 0 }}
							animate={{ opacity: 1 }}
							exit={{ opacity: 0 }}
							transition={{ duration: 0.35, ease: [0.59, 0, 0.35, 1] }}
							onClick={() => setOpen(false)}
						>
							{/* Основной контент */}
							<motion.div
								className="relative z-[10001] bg-background rounded-xl shadow-2xl max-w-4xl w-full mx-4 overflow-hidden"
								onClick={(e) => e.stopPropagation()}
								initial={{
									rotateX: -5,
									skewY: -1.5,
									scaleY: 2,
									scaleX: 0.4,
									y: 100,
								}}
								animate={{
									rotateX: 0,
									skewY: 0,
									scaleY: 1,
									scaleX: 1,
									y: 0,
									transition: {
										duration: 0.35,
										ease: [0.59, 0, 0.35, 1],
										y: { type: "spring", visualDuration: 0.7, bounce: 0.2 },
									},
								}}
								exit={{
									rotateX: -5,
									skewY: -1.5,
									scaleY: 2,
									scaleX: 0.4,
									y: 100,
								}}
								transition={{ duration: 0.35, ease: [0.59, 0, 0.35, 1] }}
								style={{
									transformPerspective: 1000,
									originX: 0.5,
									originY: 0,
								}}
							>
								{/* Заголовок */}
								<div className="p-6 border-b bg-muted/50 flex items-center justify-between">
									<div>
										<h2 className="text-2xl font-bold">Добро пожаловать!</h2>
										{showProgressBar && (
											<div className="mt-2 w-full max-w-md">
												<Progress
													value={((currentStep + 1) / steps.length) * 100}
													className="h-2"
												/>
											</div>
										)}
									</div>
									<Button
										variant="ghost"
										size="sm"
										onClick={() => setOpen(false)}
										className="shrink-0"
									>
										<X className="w-4 h-4" />
									</Button>
								</div>

								<div className="grid grid-cols-2 h-[500px]">
									{/* Левая панель - навигация */}
									<div className="p-6 border-r flex flex-col justify-between">
										<div className="space-y-3">
											{steps.map((step, index) => (
												<button
													key={index}
													onClick={() => handleStepSelect(index)}
													className={cn(
														"w-full text-left p-4 rounded-lg transition-all",
														currentStep === index
															? "bg-primary text-primary-foreground"
															: "hover:bg-muted",
														"relative",
													)}
												>
													<div className="font-medium">{step.title}</div>
													<div className="text-sm opacity-70 mt-1 line-clamp-2">
														{step.short_description}
													</div>
													{completedSteps.includes(index) && currentStep !== index && (
														<div className="absolute right-3 top-3">
															<div className="rounded-full bg-green-500 p-1">
																<CheckIcon className="w-3 h-3 text-white" />
															</div>
														</div>
													)}
												</button>
											))}
										</div>

										{/* Навигация */}
										<div className="flex items-center justify-between pt-4 border-t">
											<Button
												variant="ghost"
												onClick={handleSkip}
												className="text-muted-foreground"
											>
												Пропустить все
											</Button>
											<div className="flex gap-2">
												{currentStep > 0 && (
													<Button onClick={handlePrevious} variant="outline" size="sm">
														Назад
													</Button>
												)}
												<Button onClick={handleNext} size="sm">
													{currentStep === steps.length - 1 ? "Завершить" : "Далее"}
												</Button>
											</div>
										</div>
									</div>

									{/* Правая панель - контент */}
									<div className="p-6 flex flex-col">
										<AnimatePresence mode="wait">
											<motion.div
												key={currentStep}
												{...slideInOut(direction)}
												className="flex-1 flex flex-col"
											>
												{steps[currentStep]?.media && (
													<AspectRatio
														ratio={4 / 3}
														className="rounded-lg overflow-hidden mb-4"
													>
														{steps[currentStep]?.media?.type === "image" ? (
															<Image
																src={
																	steps[currentStep]?.media?.src || "/placeholder.svg"
																}
																alt={steps[currentStep]?.media?.alt || ""}
																fill
																className="object-cover"
															/>
														) : (
															<video
																src={steps[currentStep]?.media?.src}
																controls
																className="h-full w-full object-cover"
															/>
														)}
													</AspectRatio>
												)}

												<div className="flex-1">
													<h3 className="text-xl font-semibold mb-3">
														{steps[currentStep]?.title}
													</h3>
													<p className="text-muted-foreground mb-4">
														{steps[currentStep]?.full_description}
													</p>

													{steps[currentStep]?.action && (
														<div className="mb-4">
															{renderActionButton(steps[currentStep]?.action)}
														</div>
													)}
												</div>
											</motion.div>
										</AnimatePresence>
									</div>
								</div>
							</motion.div>
						</motion.div>
					</motion.div>
				)}
			</AnimatePresence>
		);
	}

	// Mobile версия
	return (
		<AnimatePresence>
			{open && (
				<motion.div
					className="fixed inset-0 z-[10000] bg-background"
					initial={{ opacity: 0, y: "100%" }}
					animate={{ opacity: 1, y: 0 }}
					exit={{ opacity: 0, y: "100%" }}
					transition={{ duration: 0.3, ease: [0.25, 0.1, 0.25, 1] }}
				>
					<motion.div
						drag="x"
						dragConstraints={{ left: 0, right: 0 }}
						onDragEnd={handleDragEnd}
						className="h-full flex flex-col"
					>
						{/* Заголовок */}
						<div className="p-4 border-b flex items-center justify-between">
							<div>
								<h2 className="text-xl font-bold">{steps[currentStep]?.title}</h2>
								{showProgressBar && (
									<div className="mt-2">
										<Progress value={((currentStep + 1) / steps.length) * 100} className="h-2" />
									</div>
								)}
							</div>
							<Button variant="ghost" size="sm" onClick={() => setOpen(false)}>
								<X className="w-4 h-4" />
							</Button>
						</div>

						{/* Контент */}
						<div className="flex-1 overflow-y-auto p-4">
							<AnimatePresence mode="wait">
								<motion.div key={currentStep} {...slideInOut(direction)} className="space-y-4">
									{/* Превью медиа */}
									{steps[currentStep]?.media && (
										<AspectRatio ratio={16 / 9} className="rounded-lg overflow-hidden">
											{steps[currentStep]?.media?.type === "image" ? (
												<Image
													src={steps[currentStep]?.media?.src || "/placeholder.svg"}
													alt={steps[currentStep]?.media?.alt || ""}
													fill
													className="object-cover"
												/>
											) : (
												<video
													src={steps[currentStep]?.media?.src}
													controls
													className="h-full w-full object-cover"
												/>
											)}
										</AspectRatio>
									)}

									{/* Описание */}
									<div className="space-y-3">
										<p className="text-muted-foreground">{steps[currentStep]?.full_description}</p>

										{steps[currentStep]?.action && (
											<div>{renderActionButton(steps[currentStep]?.action)}</div>
										)}
									</div>

									{/* Табы шагов */}
									<div className="grid grid-cols-2 gap-2 py-4">
										{steps.map((step, index) => (
											<button
												key={index}
												onClick={() => handleStepSelect(index)}
												className={cn(
													"p-3 rounded-lg text-left transition-all relative",
													currentStep === index
														? "bg-primary text-primary-foreground"
														: "bg-muted hover:bg-muted/80",
												)}
											>
												<div className="text-sm font-medium">{step.title}</div>
												<div className="text-xs opacity-70 mt-1 line-clamp-1">
													{step.short_description}
												</div>
												{completedSteps.includes(index) && currentStep !== index && (
													<div className="absolute right-2 top-2">
														<div className="rounded-full bg-green-500 p-0.5">
															<CheckIcon className="w-2 h-2 text-white" />
														</div>
													</div>
												)}
											</button>
										))}
									</div>
								</motion.div>
							</AnimatePresence>
						</div>

						{/* Навигация внизу */}
						<div className="border-t bg-background p-4">
							<div className="flex items-center justify-between mb-4">
								<Button variant="ghost" onClick={handleSkip} className="text-muted-foreground">
									Пропустить все
								</Button>
								<div className="flex gap-2">
									{currentStep > 0 && (
										<Button onClick={handlePrevious} variant="outline" size="sm">
											Назад
										</Button>
									)}
									<Button onClick={handleNext} size="sm">
										{currentStep === steps.length - 1 ? "Завершить" : "Далее"}
									</Button>
								</div>
							</div>
							<div className="flex items-center space-x-2">
								<Checkbox
									id="skipNextTime"
									checked={skipNextTime}
									onCheckedChange={(checked) => setSkipNextTime(checked as boolean)}
								/>
								<label htmlFor="skipNextTime" className="text-sm text-muted-foreground">
									Не показывать больше
								</label>
							</div>
						</div>
					</motion.div>
				</motion.div>
			)}
		</AnimatePresence>
	);
}
