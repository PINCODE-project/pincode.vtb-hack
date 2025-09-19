import { create } from "zustand/index";
import { CreateDatabaseModalState, DeleteDatabaseModalState } from "./interfaces";

export const useDeleteDatabaseModalStore = create<DeleteDatabaseModalState>((set) => ({
	isOpen: false,
	databaseId: null,
	databaseName: null,
	open: (databaseId: string, databaseName: string) => set({ isOpen: true, databaseId, databaseName }),
	close: () => set({ isOpen: false, databaseId: null, databaseName: null }),
	toggle: () => set((state: DeleteDatabaseModalState) => ({ isOpen: !state.isOpen })),
}));
