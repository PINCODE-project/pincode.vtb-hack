import { create } from "zustand/index";
import { OnboardingState } from "./interfaces";

export const useOnboardingStore = create<OnboardingState>((set) => ({
	isOpen: false,
	setIsOpen: (isOpen) => set({ isOpen }),
	open: () => set({ isOpen: true }),
	close: () => set({ isOpen: false }),
	toggle: () => set((state) => ({ isOpen: !state.isOpen })),
}));
