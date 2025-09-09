export type OnboardingState = {
	isOpen: boolean;
	setIsOpen: (isOpen: boolean) => void;
	open: () => void;
	close: () => void;
	toggle: () => void;
};
