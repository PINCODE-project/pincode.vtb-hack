import { create } from "zustand/index";
import { CreateProjectModalState } from "./interfaces";

export const useCreateProjectModalStore = create<CreateProjectModalState>((set) => ({
	isOpen: false,
	open: () => set({ isOpen: true }),
	close: () => set({ isOpen: false }),
	toggle: () => set((state) => ({ isOpen: !state.isOpen })),
}));
