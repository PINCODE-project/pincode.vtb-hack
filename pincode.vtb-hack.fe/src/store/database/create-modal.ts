import { create } from "zustand/index";
import { CreateDatabaseModalState } from "./interfaces";

export const useCreateDatabaseModalStore = create<CreateDatabaseModalState>((set) => ({
	isOpen: false,
	open: () => set({ isOpen: true }),
	close: () => set({ isOpen: false }),
	toggle: () => set((state: CreateDatabaseModalState) => ({ isOpen: !state.isOpen })),
}));
