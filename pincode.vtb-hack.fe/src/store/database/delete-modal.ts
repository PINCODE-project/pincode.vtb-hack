import { create } from "zustand/index";
import { CreateDatabaseModalState, DeleteDatabaseModalState } from "./interfaces";

export const useDeleteDatabaseModalStore = create<DeleteDatabaseModalState>((set) => ({
	isOpen: false,
	open: () => set({ isOpen: true }),
	close: () => set({ isOpen: false }),
	toggle: () => set((state: CreateDatabaseModalState) => ({ isOpen: !state.isOpen })),
}));
