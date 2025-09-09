import { create } from "zustand";
import type { DbConnectionDto } from "@generated";

interface EditDatabaseModalState {
	isOpen: boolean;
	database: DbConnectionDto | null;
	open: (database: DbConnectionDto) => void;
	close: () => void;
}

export const useEditDatabaseModalStore = create<EditDatabaseModalState>((set) => ({
	isOpen: false,
	database: null,
	open: (database) => set({ isOpen: true, database }),
	close: () => set({ isOpen: false, database: null }),
}));
